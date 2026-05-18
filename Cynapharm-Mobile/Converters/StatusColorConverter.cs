using System.Globalization;

namespace Cynapharm_Mobile.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "BROUILLON"      => Color.FromArgb("#9E9E9E"),
            "EN_ATTENTE"     => Color.FromArgb("#F5A623"),
            "CONFIRMEE"      => Color.FromArgb("#00B4D8"),
            "EN_PREPARATION" => Color.FromArgb("#185FA5"),
            "EXPEDIEE"       => Color.FromArgb("#6C5CE7"),
            "LIVREE"         => Color.FromArgb("#2d7a4f"),
            "ANNULEE"        => Color.FromArgb("#E24B4A"),
            _                => Color.FromArgb("#9E9E9E")
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
