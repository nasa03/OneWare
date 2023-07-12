﻿using Avalonia.Media;
using DynamicData;

namespace OneWare.Shared.EditorExtensions;

public class ScrollInfoContext
{
    private readonly Dictionary<string, ScrollInfoLine[]> _scrollInformation = new();

    private readonly List<ScrollInfoLine> _infoLines = new List<ScrollInfoLine>();
    public IReadOnlyList<ScrollInfoLine> InfoLines => _infoLines;
    
    public event EventHandler? Changed;

    private void Clear(string key)
    {
        _scrollInformation.TryGetValue(key, out var inf);
        if (inf != null)
        {
            _infoLines.RemoveMany(inf);
        }
        _scrollInformation.Remove(key);
    }
    
    public void Refresh(string key, params ScrollInfoLine[] lines)
    {
        Clear(key);
        _scrollInformation[key] = lines;
        _infoLines.AddRange(lines);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

public class ScrollInfoLine
{
    public int Line { get; }
    public IBrush Brush { get; }

    public ScrollInfoLine(int line, IBrush brush)
    {
        Line = line;
        Brush = brush;
    }
}