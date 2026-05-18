namespace Cynapharm_Mobile.Services;

public class ShellNavigationService : INavigationService
{
    public Task GoToAsync(string route)
        => Shell.Current.GoToAsync(route);

    public Task GoToAsync<TParam>(string route, TParam param) where TParam : class
    {
        var navParam = new Dictionary<string, object> { ["param"] = param };
        return Shell.Current.GoToAsync(route, navParam);
    }

    public Task GoBackAsync()
        => Shell.Current.GoToAsync("..");

    public Task GoToRootAsync(string rootRoute = "//login")
        => Shell.Current.GoToAsync(rootRoute);
}
