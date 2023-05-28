﻿using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using OneWare.SerialMonitor.ViewModels;
using OneWare.Output.Views;

namespace OneWare.SerialMonitor.Views
{
    public partial class SerialMonitorView : OutputBaseView
    {
        public SerialMonitorView()
        {
            InitializeComponent();

            CommandBox.AddHandler(KeyDownEvent, (sender, args) =>
            {
                if (DataContext is SerialMonitorBaseViewModel vm)
                {
                    if (args.Key == Key.Up) vm.InsertUp();
                    else if (args.Key == Key.Down) vm.InsertDown();
                }
            }, RoutingStrategies.Direct);
        }
    }
}