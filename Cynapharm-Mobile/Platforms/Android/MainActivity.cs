using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace Cynapharm_Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            // Capture managed exceptions that become JavaProxyThrowable
            AndroidEnvironment.UnhandledExceptionRaiser += OnUnhandledManagedException;
            base.OnCreate(savedInstanceState);
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = new AndroidExceptionHandler();
        }

        private static void OnUnhandledManagedException(object? sender, RaiseThrowableEventArgs args)
        {
            var ex = args.Exception;
            Android.Util.Log.Error("CYNAPHARM_CRASH", $"Type   : {ex?.GetType()?.FullName}");
            Android.Util.Log.Error("CYNAPHARM_CRASH", $"Message: {ex?.Message}");
            Android.Util.Log.Error("CYNAPHARM_CRASH", $"Source : {ex?.Source}");
            Android.Util.Log.Error("CYNAPHARM_CRASH", $"Stack  : {ex?.StackTrace}");
            if (ex?.InnerException != null)
                Android.Util.Log.Error("CYNAPHARM_CRASH", $"Inner  : {ex.InnerException}");
            args.Handled = false; // let Android kill the app normally
        }
    }

    public class AndroidExceptionHandler : Java.Lang.Object,
        Java.Lang.Thread.IUncaughtExceptionHandler
    {
        public void UncaughtException(Java.Lang.Thread t, Java.Lang.Throwable e)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[JAVA UNCAUGHT] {e.GetType().Name}: {e.Message}\n{e.StackTrace}");
            var cause = e.Cause;
            while (cause != null)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[JAVA CAUSE] {cause.GetType().Name}: {cause.Message}");
                cause = cause.Cause;
            }
        }
    }
}
