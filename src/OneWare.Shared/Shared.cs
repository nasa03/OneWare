using System.Runtime.InteropServices;
using Avalonia;

namespace OneWare.Shared;

public static class Shared
{
    public static Thickness WindowsOnlyBorder => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Thickness(1) : new
        Thickness(0);

    public static CornerRadius WindowsCornerRadius => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? (Environment.OSVersion.Version.Build >= 22000 ? new CornerRadius(8) : new CornerRadius(0))
        : new CornerRadius(0);
        
    public static CornerRadius WindowsCornerRadiusBottom => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? (Environment.OSVersion.Version.Build >= 22000 ? new CornerRadius(0,0,8,8) : new CornerRadius(0))
        : new CornerRadius(0);
        
    public static CornerRadius WindowsCornerRadiusBottomLeft => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? (Environment.OSVersion.Version.Build >= 22000 ? new CornerRadius(0,0,0,8) : new CornerRadius(0))
        : new CornerRadius(0);
        
    public static CornerRadius WindowsCornerRadiusBottomRight => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? (Environment.OSVersion.Version.Build >= 22000 ? new CornerRadius(0,0,8,0) : new CornerRadius(0))
        : new CornerRadius(0);
}