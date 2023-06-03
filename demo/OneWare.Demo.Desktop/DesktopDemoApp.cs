﻿using OneWare.SourceControl;
using OneWare.Terminal;
using Prism.Modularity;

namespace OneWare.Demo.Desktop;

public class DesktopDemoApp : DemoApp
{
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        base.ConfigureModuleCatalog(moduleCatalog);
        moduleCatalog.AddModule<TerminalModule>();
        moduleCatalog.AddModule<SourceControlModule>();
    }
}