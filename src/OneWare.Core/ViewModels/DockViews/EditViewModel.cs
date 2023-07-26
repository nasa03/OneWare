﻿using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using DryIoc;
using DynamicData.Binding;
using OneWare.Core.Services;
using Prism.Ioc;
using OneWare.Shared;
using OneWare.Shared.EditorExtensions;
using OneWare.Shared.Enums;
using OneWare.Shared.LanguageService;
using OneWare.Shared.Models;
using OneWare.Shared.Services;
using OneWare.Shared.Views;

namespace OneWare.Core.ViewModels.DockViews
{
    public class EditViewModel : ExtendedDocument, IEditor
    {
        private static readonly IBrush ErrorBrushText = new SolidColorBrush(Color.FromArgb(255, 175, 50, 50));
        private static readonly IBrush ErrorBrush = new SolidColorBrush(Color.FromArgb(150, 175, 50, 50));
        private static readonly IBrush WarningBrush = new SolidColorBrush(Color.FromArgb(150, 155, 155, 0));
        
        private readonly IDockService _dockService;
        private readonly ILanguageManager _languageManager;
        private readonly ISettingsService _settingsService;
        private readonly IErrorService _errorService;
        private readonly IWindowService _windowService;
        private readonly IProjectExplorerService _projectExplorerService;
        private readonly BackupService _backupService;

        public bool DisableEditViewEvents { get; private set; }
        
        private CompositeDisposable _composite = new();
        
        public ExtendedTextEditor Editor { get; } = new();

        private ITypeAssistance? _typeAssistance;

        public ITypeAssistance? TypeAssistance
        {
            get => _typeAssistance;
            private set => SetProperty(ref _typeAssistance, value);
        }

        public TextDocument CurrentDocument => Editor.Document;

        public ScrollInfoContext ScrollInfo { get; } = new();
        

        private IEnumerable<ErrorListItemModel>? _diagnostics;
        
        public IEnumerable<ErrorListItemModel>? Diagnostics
        {
            get => _diagnostics;
            set => SetProperty(ref _diagnostics, value);
        }

        public event EventHandler? FileSaved;

        public EditViewModel(string fullPath, ILogger logger, ISettingsService settingsService,
            IDockService dockService, ILanguageManager languageManager, IWindowService windowService,
            IProjectExplorerService projectExplorerService, IErrorService errorService, BackupService backupService) : base(fullPath, projectExplorerService, dockService, windowService)
        {
            _settingsService = settingsService;
            _dockService = dockService;
            _windowService = windowService;
            _projectExplorerService = projectExplorerService;
            _languageManager = languageManager;
            _errorService = errorService;
            _backupService = backupService;
            
            Title = $"Loading {Path.GetFileName(fullPath)}";

            logger.Log("Initializing " + fullPath + "", ConsoleColor.DarkGray);
            
            Undo = new RelayCommand(() => Editor.Undo());
            Redo = new RelayCommand(() => Editor.Redo());

            this.WhenValueChanged(x => x.Diagnostics).Subscribe(x =>
            {
                List<ScrollInfoLine> scrollInfo = new();

                if (_diagnostics != null)
                {
                    Editor.MarkerService.SetDiagnostics(_diagnostics);
                    
                    // Editor.ModificationService.SetModification("Errors", _diagnostics.Where(x => x.Type == ErrorType.Error).Select(b =>
                    // {
                    //     var off = b.GetOffset(Editor.Document);
                    //     return new TextModificationService.TextModificationSegment(off.startOffset, off.endOffset)
                    //     {
                    //         Brush = _errorBrushText
                    //     };
                    // }).ToArray());
                    
                    var errorLines = _diagnostics
                        .Where(b => b.Type is ErrorType.Error)
                        .Select(c => c.StartLine)
                        .Distinct();

                    var warningLines = _diagnostics
                        .Where(b => b.Type is ErrorType.Warning)
                        .Select(c => c.StartLine)
                        .Distinct();

                    scrollInfo.AddRange(warningLines.Select(l => new ScrollInfoLine(l, WarningBrush)));
                    scrollInfo.AddRange(errorLines.Select(l => new ScrollInfoLine(l, ErrorBrush)));
                }
                
                ScrollInfo.Refresh("ErrorContext", scrollInfo.ToArray());
                Editor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            });
            
            _errorService.ErrorRefresh += (sender, o) =>
            {
                if(CurrentFile != null && o == CurrentFile) Diagnostics = _errorService.GetErrorsForFile(CurrentFile);
            };
        }
        
        protected override void ChangeCurrentFile(IFile? oldFile)
        {
            Reset();
            
            if (CurrentFile == null) throw new NullReferenceException(nameof(CurrentFile));

            Diagnostics = _errorService.GetErrorsForFile(CurrentFile);

            async void OnInitialized()
            {
                var result = await LoadAsync();

                DisableEditViewEvents = CurrentDocument.TextLength > 100000;
                if (DisableEditViewEvents)
                {
                    ContainerLocator.Container.Resolve<ILogger>().Warning("Some features are disabled for this large file to reduce performance loss");
                }
                else
                {
                    var scope = _languageManager.GetTextMateScopeByExtension(CurrentFile.Extension);
                    if (scope != null)
                    {
                        if(Editor.TextMateInstallation == null) Editor.InitTextmate(_languageManager.RegistryOptions);
                        Editor.TextMateInstallation?.SetGrammar(scope);
                        _languageManager.WhenValueChanged(x => x.CurrentEditorTheme).Subscribe(x =>
                        {
                            Editor.TextMateInstallation?.SetTheme(x);
                        }).DisposeWith(_composite);
                    }
                    else
                    {
                        if (Editor.TextMateInstallation != null)
                        {
                            Editor.RemoveTextmate();
                        }
                    }
                
                    Observable.FromEventPattern(
                            h => Editor.Document.TextChanged += h,
                            h => Editor.Document.TextChanged -= h)
                        .Subscribe(x => { IsDirty = true; }).DisposeWith(_composite);
                
                    if(result) InitTypeAssistance();
                }
            }
            OnInitialized();
        }

        private void InitTypeAssistance()
        {
            if(CurrentFile == null) return;
            
            TypeAssistance = _languageManager.GetTypeAssistance(this);

            if (TypeAssistance != null)
            {
                Editor.SetEnableBreakpoints(TypeAssistance.CanAddBreakPoints, CurrentFile);

                if (TypeAssistance.FoldingStrategy != null)
                {
                    _settingsService.GetSettingObservable<bool>("Editor_UseFolding").Subscribe(x =>
                    {
                        Editor.SetEnableFolding(x);
                        if (x) UpdateFolding();
                    }).DisposeWith(_composite);
                    
                    Observable.FromEventPattern(
                            h => Editor.Document.LineCountChanged += h,
                            h => Editor.Document.LineCountChanged -= h)
                        .Subscribe(x => { UpdateFolding(); }).DisposeWith(_composite);
                }
                
                // Observable.FromEventPattern(
                //         h => TypeAssistance.AssistanceActivated += h,
                //         h => TypeAssistance.AssistanceActivated -= h)
                //     .Subscribe(x =>
                //     {
                //         
                //     });
                //
                // Observable.FromEventPattern(
                //         h => TypeAssistance.AssistanceDeactivated += h,
                //         h => TypeAssistance.AssistanceDeactivated -= h)
                //     .Subscribe(x => {  });
                
                TypeAssistance.Open();
            }
        }

        private void UpdateFolding()
        {
            if (_settingsService.GetSettingValue<bool>("Editor_UseFolding") && Editor.FoldingManager != null)
                TypeAssistance?.FoldingStrategy?.UpdateFoldings(Editor.FoldingManager, CurrentDocument);
        }

        #region Jump

        public async Task<bool> WaitForEditorReadyAsync()
        {
            const int timeOut = 1000;
            var now = DateTime.Now.Millisecond;
            while (DateTime.Now.Millisecond - now < timeOut)
            {
                if (Editor is { IsInitialized: true }) return true;
                await Task.Delay(100);
            }

            return false;
        }

        public void JumpToLine(int lineNumber, bool select = true)
        {
            _ = JumpToLineAsync(lineNumber, select);
        }

        private async Task JumpToLineAsync(int lineNumber, bool select = true)
        {
            if (!await WaitForEditorReadyAsync()) return;
            await Task.Delay(100);
            if (lineNumber <= CurrentDocument.LineCount)
            {
                var line = CurrentDocument.GetLineByNumber(lineNumber);
                if (select) Editor.Select(line.Offset, line.Length);
                Editor.CaretOffset = line.Offset;
                Editor.TextArea.Caret.BringCaretToView(Editor.ViewportHeight / 3);
            }
        }

        public void Select(int offset, int length)
        {
            _ = SelectAsync(offset, length);
        }

        private async Task SelectAsync(int offset, int length)
        {
            if (!await WaitForEditorReadyAsync()) return;
            await Task.Delay(100);
            if (offset + length <= Editor.Text.Length)
            {
                Editor.Select(offset, length);
                Editor.CaretOffset = offset;
                Editor.TextArea.Caret.BringCaretToView(Editor.ViewportHeight / 2);
            }
        }

        #endregion

        #region LoadAndSave

        protected override void Reset()
        {
            TypeAssistance?.Close();
            _composite.Dispose();
            _composite = new CompositeDisposable();
        }

        public override async Task<bool> SaveAsync()
        {
            if (IsReadOnly || CurrentFile == null) return true;
            
            try
            {
                await Tools.WriteTextFileAsync(CurrentFile.FullPath, CurrentDocument.Text);
            }
            catch (Exception e)
            {
                ContainerLocator.Container.Resolve<ILogger>()?.Error(e.Message, e);
                return false;
            }

            ContainerLocator.Container.Resolve<ILogger>()
                ?.Log($"Saved {CurrentFile.Header}!", ConsoleColor.Green);
            
            IsDirty = false;
            CurrentFile.LastSaveTime = DateTime.Now;
            CurrentFile.LoadingFailed = false;
            LoadingFailed = false;
            
            FileSaved?.Invoke(this, EventArgs.Empty);
            
            return true;
        }

        private async Task<bool> LoadAsync()
        {
            if (CurrentFile == null) return false;

            IsLoading = true;
            
            var success = true;
            try
            {
                await using var stream = File.OpenRead(CurrentFile.FullPath);
                using var reader = new StreamReader(stream);
                
                var doc = await Task.Run(() =>
                {
                    var d = new TextDocument(reader.ReadToEnd());
                    d.SetOwnerThread(null);
                    return d;
                });

                doc.SetOwnerThread(Thread.CurrentThread);

                Editor.Document = doc;
                
                CurrentFile.LastSaveTime = File.GetLastWriteTime(CurrentFile.FullPath);
                
                CurrentDocument.UndoStack.ClearAll();
            }
            catch (Exception e)
            {
                ContainerLocator.Container.Resolve<ILogger>()
                    ?.Error($"Failed loading file {CurrentFile.FullPath}", e, false);
                
                success = false;
            }
            
            IsLoading = false;
            CurrentFile.LoadingFailed = !success;
            LoadingFailed = !success;
            IsDirty = false;
            
            if (success) _ = _backupService.SearchForBackupAsync(CurrentFile);
            
            return success;
        }

        #endregion

        public void AutoIndent()
        {
            TypeAssistance?.AutoIndent();
        }
        
        public void Format()
        {
            TypeAssistance?.Format();
        }

        public void Comment()
        {
            TypeAssistance?.Comment();
        }

        public void Uncomment()
        {
            TypeAssistance?.Uncomment();
        }
    }
}