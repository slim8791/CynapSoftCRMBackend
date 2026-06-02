namespace Cynapharm_Mobile.Services;

public static class HapticService
{
    public static void Light()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.Click); }
        catch { /* Silently ignore on platforms that don't support haptics */ }
    }

    public static void Success()
    {
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); }
        catch { }
    }

    public static void Error()
    {
        try
        {
            HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);
        }
        catch { }
    }
}
