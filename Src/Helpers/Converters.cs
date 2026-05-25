using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace ZumenSearch.Helpers.Converters;

// not really used
public partial class EnumToBooleanConverter : IValueConverter
{
    // Converts Enum (or string) to Boolean
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null || parameter == null)
            return false;

        string? enumValue = value?.ToString();
        string? targetValue = parameter?.ToString();

        return string.Equals(enumValue, targetValue, StringComparison.OrdinalIgnoreCase);
    }

    // Converts Boolean back to Enum
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isChecked && isChecked && parameter != null)
        {
            // Parse the target enum type
            //return Enum.Parse(targetType, parameter.ToString());
            return Enum.Parse(targetType, parameter.ToString() ?? string.Empty);
        }

        return DependencyProperty.UnsetValue;
    }
}

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
