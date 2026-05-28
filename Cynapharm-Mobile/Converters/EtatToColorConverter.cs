using System.Globalization;

namespace Cynapharm_Mobile.Converters;

/// <summary>
/// Converts a Planning Etat integer code to a background colour for the status badge.
/// 0 = EnAttente (amber), 1 = Confirme (green), 2 = Annule (red)
/// </summary>
public class EtatToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int code) return Color.FromArgb("#9E9E9E");
        return code switch
        {
            0 => Color.FromArgb("#FF8F00"), // EnAttente — amber
            1 => Color.FromArgb("#2E7D32"), // Confirme  — green
            2 => Color.FromArgb("#C62828"), // Annule    — red
            _ => Color.FromArgb("#9E9E9E")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
