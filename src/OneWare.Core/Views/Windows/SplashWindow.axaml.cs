﻿using Avalonia;
using Avalonia.Controls;

namespace OneWare.Core.Views.Windows
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
    }
}