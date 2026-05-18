namespace Cynapharm_Mobile.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
    Task GoToAsync<TParam>(string route, TParam param) where TParam : class;
    Task GoBackAsync();
    Task GoToRootAsync(string rootRoute = "//login");
}
