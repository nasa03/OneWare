﻿using OneWare.Shared.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace OneWare.Vhdl;

public class VhdlModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        containerProvider.Resolve<IErrorService>().RegisterErrorSource("VHDL LS");
        //containerProvider.Resolve<ILanguageManager>().RegisterHighlighting("avares://OneWare.Vhdl/Assets/vhdl.xshd", ".vhd", ".vhdl");
        containerProvider.Resolve<ILanguageManager>().RegisterTextMateLanguage("source.vhdl", "avares://OneWare.Vhdl/Assets/vhdl.tmLanguage.json", ".vhd", ".vhdl");
        containerProvider.Resolve<ILanguageManager>().RegisterService(typeof(LanguageServiceVhdl),true, ".vhd", ".vhdl");
    }
}