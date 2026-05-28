using System.Globalization;

namespace Cynapharm_Mobile.Converters;

/// <summary>
/// Converts a Planning Etat integer code to a human-readable French label.
/// 0 = EnAttente, 1 = Confirme, 2 = Annule
/// </summary>
public class EtatToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int code) return "Inconnu";
        return code switch
        {
            0 => "En attente",
            1 => "Confirmé",
            2 => "Annulé",
            _ => $"Etat {code}"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
