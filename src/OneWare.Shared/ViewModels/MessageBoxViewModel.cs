﻿using System.Collections.ObjectModel;
using Avalonia.Controls;


namespace OneWare.Shared.ViewModels
{
    public enum MessageBoxStatus
    {
        Canceled,
        Yes,
        No
    }

    public enum MessageBoxMode
    {
        AllButtons,
        NoCancel,
        OnlyOk,
        Input,
        PasswordInput,
        SelectFolder,
        SelectItem
    }

    public enum MessageBoxIcon
    {
        Info,
        Warning,
        Error
    }

    public class MessageBoxViewModel : ViewModelBase
    {
        private string _input = "";

        private object _selectedItem;

        private ObservableCollection<object> _selectionItems = new();
        
        public MessageBoxViewModel(string title, string message, MessageBoxMode mode)
        {
            Title = title;
            Message = message;

            //TODO USE FLAGS

            if (mode == MessageBoxMode.PasswordInput) PasswordChar = "*";
            else if (mode == MessageBoxMode.SelectFolder) ShowFolderButton = true;

            switch (mode)
            {
                case MessageBoxMode.NoCancel:
                    ShowCancel = false;
                    break;

                case MessageBoxMode.Input:
                case MessageBoxMode.PasswordInput:
                case MessageBoxMode.SelectFolder:
                    ShowInput = true;
                    ShowOk = true;
                    ShowYes = false;
                    ShowNo = false;
                    break;

                case MessageBoxMode.OnlyOk:
                    ShowInput = false;
                    ShowOk = true;
                    ShowYes = false;
                    ShowNo = false;
                    ShowCancel = false;
                    break;

                case MessageBoxMode.SelectItem:
                    ShowSelection = true;
                    ShowOk = true;
                    ShowYes = false;
                    ShowNo = false;
                    break;
            }
        }

        public MessageBoxStatus BoxStatus { get; set; } = MessageBoxStatus.Canceled;
        public string Title { get; } = "";
        public string Message { get; } = "";

        public string Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public bool ShowYes { get; } = true;
        public bool ShowNo { get; } = true;
        public bool ShowCancel { get; } = true;
        public bool ShowOk { get; }
        public bool ShowInput { get; }
        public bool ShowFolderButton { get; }
        public bool ShowSelection { get; }

        public ObservableCollection<object> SelectionItems
        {
            get => _selectionItems;
            set => SetProperty(ref _selectionItems, value);
        }

        public object SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public string PasswordChar { get; } = "";

        public void No(Window window)
        {
            BoxStatus = MessageBoxStatus.No;
            Close(window);
        }

        public void Yes(Window window)
        {
            BoxStatus = MessageBoxStatus.Yes;
            Close(window);
        }

        public void Cancel(Window window)
        {
            BoxStatus = MessageBoxStatus.Canceled;
            Close(window);
        }
        
        public async Task SelectPathAsync(Window window)
        {
            var folder = await Tools.SelectFolderAsync(window, "Select Directory", Input);
            
            Input = folder ?? "";
        }
    }
}