﻿using Avalonia.Data.Converters;

namespace OneWare.Shared.Converters;

public static class SharedConverters
{
    public static readonly IValueConverter BoolToOpacityConverter = new BoolToOpacityConverter();
    public static readonly IValueConverter
        BoolToScrollBarVisibilityConverter = new BoolToScrollBarVisibilityConverter();

    public static readonly IValueConverter ComparisonConverter = new ComparisonConverter();
    public static readonly IValueConverter EnumToStringConverter = new EnumToStringConverter();
    public static readonly IValueConverter FileExtensionIconConverter = new FileExtensionIconConverter();
    public static readonly IValueConverter FileExtensionIconConverterObservable = new FileExtensionIconConverterObservable();
    public static readonly IValueConverter FileOpacityConverter = new FileOpacityConverter();
    public static readonly IValueConverter NoComparisonConverter = new NoComparisonConverter();
    public static readonly IValueConverter ObjectNotTypeConverter = new ObjectNotTypeConverter();
    public static readonly IValueConverter ObjectTypeConverter = new ObjectTypeConverter();
    public static readonly IValueConverter TimeUnitConverter = new TimeUnitConverter();
    public static readonly IValueConverter PathToWindowIconConverter = new PathToWindowIconConverter();
    public static readonly PathToBitmapConverter PathToBitmapConverter = new();
    public static readonly IMultiValueConverter PathsEqualConverter = new PathsEqualConverter();
}