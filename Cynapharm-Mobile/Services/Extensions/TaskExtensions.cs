namespace Cynapharm_Mobile.Services;

public static class TaskExtensions
{
    public static async void SafeFireAndForget(
        this Task task,
        Action<Exception>? onError = null)
    {
        try
        {
            await task;
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex);
            System.Diagnostics.Debug.WriteLine($"[TASK ERROR] {ex}");
        }
    }
}
