using System.Globalization;
using System.Windows;
using System.Windows.Data;
using PdfEdit.Core;

namespace PdfEditApp.Converters;

public sealed class OperationVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not PdfOperationType operation)
        {
            return Visibility.Collapsed;
        }

        if (parameter is string name && Enum.TryParse<PdfOperationType>(name, out var expected))
        {
            return operation == expected ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
