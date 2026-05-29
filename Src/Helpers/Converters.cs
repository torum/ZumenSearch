using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Globalization;

namespace ZumenSearch.Helpers.Converters;

// not used for now.
public partial class ThemeEnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            if (!Enum.IsDefined(typeof(ElementTheme), value))
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse<ElementTheme>(enumString);

            return enumValue.Equals(value);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse<ElementTheme>(enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}

// not really used.
// BorderBrush="{x:Bind ViewModel.HasNameErrors, Mode=OneWay, Converter={StaticResource ErrorBrushConverter}}"
public partial class ErrorToBorderBrushConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        bool hasError = value is bool v && v;

        // Return Red brush if there is an error, otherwise rely on the default Theme Brush
        return hasError
            ? new SolidColorBrush(Colors.Red)
            : null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
