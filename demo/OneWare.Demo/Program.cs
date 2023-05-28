﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Dialogs;
using Avalonia.Media;
using OneWare.Core;
using OneWare.Core.Data;
using OneWare.Shared;
using OneWare.Shared.Services;
using Prism.Ioc;

namespace OneWare.Demo;

internal abstract class Program
{
    // This method is needed for IDE previewer infrastructure
    private static AppBuilder BuildAvaloniaApp()
    {
        App.SettingsService.Register("LastVersion", Global.VersionCode);
        App.SettingsService.RegisterSettingCategory("Experimental", 100, "MaterialDesign.Build");
        App.SettingsService.RegisterTitled("Experimental", "Misc", "Experimental_UseManagedFileDialog", "Use Managed File Dialog",
            "", RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
        App.SettingsService.Load(DemoApp.Paths.SettingsPath);

        var app = AppBuilder.Configure<DemoApp>().UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                EnableMultiTouch = true
            })
            .With(new Win32PlatformOptions
            {
                CompositionBackdropCornerRadius = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? (Environment.OSVersion.Version.Build >= 22000 ? 8 : 0)
                    : 0,
            })
            .With(new FontManagerOptions
            {
                DefaultFamilyName = "avares://OneWare.Core/Assets/Fonts#Noto Sans"
            })
            .LogToTrace();

        if (App.SettingsService.GetSettingValue<bool>("Experimental_UseManagedFileDialog")) app.UseManagedSystemDialogs();

        return app;
    }

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            //Create all default directories if not existing              
            Directory.CreateDirectory(DemoApp.Paths.CrashReportsDirectory);
            Directory.CreateDirectory(DemoApp.Paths.LibrariesDirectory);
            Directory.CreateDirectory(DemoApp.Paths.ProjectsDirectory);
            Directory.CreateDirectory(DemoApp.Paths.AppDataDirectory);
            Directory.CreateDirectory(DemoApp.Paths.PackagesDirectory);


            return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            if(ContainerLocator.Container.IsRegistered<ILogger>())
                ContainerLocator.Container?.Resolve<ILogger>()?.Error(ex.Message, ex, false);
            else Console.WriteLine(ex.ToString());

            Tools.WriteTextFile(
                Path.Combine(DemoApp.Paths.CrashReportsDirectory,
                    "crash_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", DateTimeFormatInfo.InvariantInfo) +
                    ".txt"), ex.ToString());
#if DEBUG
            Console.ReadLine();
#endif
        }

        return 0;
    }
}