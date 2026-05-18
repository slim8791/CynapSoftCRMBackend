using System.Globalization;

namespace Cynapharm_Mobile.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "EN_ATTENTE" => Color.FromArgb("#F57C00"),
            "CONFIRMEE"  => Color.FromArgb("#1A6B3C"),
            "LIVREE"     => Color.FromArgb("#388E3C"),
            "ANNULEE"    => Color.FromArgb("#D32F2F"),
            _            => Color.FromArgb("#9E9E9E")
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
