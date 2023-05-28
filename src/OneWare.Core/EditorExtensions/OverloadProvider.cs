﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvaloniaEdit.CodeCompletion;

namespace OneWare.Core.EditorExtensions
{
    public class OverloadProvider : IOverloadProvider
    {
        private readonly IList<(string header, string? content)> _items;
        private int _selectedIndex;

        public OverloadProvider(IList<(string header, string? content)> items)
        {
            _items = items;
            SelectedIndex = 0;
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentHeader));
                OnPropertyChanged(nameof(CurrentContent));
                OnPropertyChanged(nameof(CurrentIndexText));
            }
        }

        public int Count => _items.Count;
        public string CurrentIndexText => SelectedIndex + 1 + "/" + _items.Count;
        public object CurrentHeader => _items[SelectedIndex].header;
        public object? CurrentContent => _items[SelectedIndex].content;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}