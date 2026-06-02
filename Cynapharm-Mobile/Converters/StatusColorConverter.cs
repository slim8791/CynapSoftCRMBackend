using System.Globalization;

namespace Cynapharm_Mobile.Converters;

public class StatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int code) return Color.FromArgb("#9E9E9E");
        return code switch
        {
            0 => Color.FromArgb("#9E9E9E"), // Brouillon
            1 => Color.FromArgb("#F5A623"), // En attente
            2 => Color.FromArgb("#00B4D8"), // Confirmée
            3 => Color.FromArgb("#185FA5"), // En préparation
            4 => Color.FromArgb("#6C5CE7"), // Expédiée
            5 => Color.FromArgb("#2d7a4f"), // Livrée
            6 => Color.FromArgb("#E24B4A"), // Annulée
            _ => Color.FromArgb("#9E9E9E")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
