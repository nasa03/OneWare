﻿
using Dock.Model.Mvvm.Controls;
using OneWare.Shared.Controls;

namespace OneWare.Shared.ViewModels;

public class FlexibleWindowViewModelBase : Document
{
    public void Close(FlexibleWindow window)
    {
        window.Close();
    }
}