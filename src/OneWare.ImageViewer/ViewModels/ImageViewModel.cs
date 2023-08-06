using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using OneWare.Shared.Models;
using OneWare.Shared.Services;
using OneWare.Shared.ViewModels;
using Prism.Ioc;

namespace OneWare.ImageViewer.ViewModels;

public class ImageViewModel : ExtendedDocument
{
    private IImage? _image;

    public IImage? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }
    
    public ImageViewModel(string fullPath, IProjectExplorerService projectExplorerService, IDockService dockService, IWindowService windowService) : 
        base(fullPath, projectExplorerService, dockService, windowService)
    {
    }

    protected override void UpdateCurrentFile(IFile? oldFile)
    {
        if (CurrentFile is null) throw new NullReferenceException(nameof(CurrentFile));

        try
        {
            switch (CurrentFile.Extension.ToLower())
            {
                case ".svg":
                    var svg = new SvgSource();
                    var picture = svg.Load(FullPath);
                    if (picture is { })
                    {
                        Image = new SvgImage()
                        {
                            Source = svg
                        };
                    }
                    break;
                case ".jpg":
                case ".png":
                    Image = new Bitmap(FullPath);
                    break;
            }
        }
        catch (Exception e)
        {
            ContainerLocator.Container.Resolve<ILogger>().Error(e.Message,e);
        }
        IsLoading = false;
    }
}