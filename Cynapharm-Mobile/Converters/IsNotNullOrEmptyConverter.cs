using System.Globalization;

namespace Cynapharm_Mobile.Converters;

public class IsNotNullOrEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // bool with "TrueText|FalseText" parameter → return the matching string label
        if (value is bool b && parameter is string opts)
        {
            var parts = opts.Split('|');
            return b ? (parts.Length > 0 ? parts[0] : string.Empty)
                     : (parts.Length > 1 ? parts[1] : string.Empty);
        }

        // string → bool (original behaviour)
        if (value is string s) return !string.IsNullOrEmpty(s);

        // null → false
        if (value is null) return false;

        // any other non-null value (objects, decimal?, etc.) → true
        return true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
