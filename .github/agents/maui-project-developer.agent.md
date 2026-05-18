---
name: maui-project-developer
user-invocable: false
description: "Internal subagent for Cynapharm-Mobile MAUI implementation. Builds views, view models, services, and app shell integration from the planner output. Not user-invokable directly."
tools:
  - create_file
  - insert_edit_into_file
  - get_errors
  - vscode/askQuestions
---

# .NET MAUI Project Developer

This internal agent is part of `maui-project-orchestrator` and implements the Cynapharm-Mobile MAUI application based on the planner's recommendations.

## Responsibilities
- Generate MAUI views and view models using recommended MVVM organization.
- Create the folder structure defined in the plan.
- Create views with XAML layout and minimal code-behind (constructor + BindingContext only).
- Create view models inheriting `BaseViewModel`, using `[ObservableProperty]` and `[RelayCommand]`.
- Create services implementing their `IXxxService` interface, inheriting `ApiServiceBase`.
- Register all services, view models, and pages in `MauiProgram.cs`.
- Update `AppShell.xaml` and `AppShell.xaml.cs` with shell routes and navigation entry points.
- Add NuGet packages `CommunityToolkit.Mvvm` and `Microsoft.Extensions.Http` to `.csproj`.
- Fix compile errors as needed while implementing the app.
- Validate that naming conventions and view/view model separation match the plan.

---

## Implementation Order (7 Passes — must follow to avoid forward-reference errors)

```
Pass 1: Infrastructure        ResponseDto, PagedResult, ITokenManager, TokenManager,
                              JwtAuthHandler, ApiServiceBase, BaseViewModel
Pass 2: Domain Models         Auth, Products, Orders, Field, Inventory, Documents
Pass 3: Service Interfaces    IAuthService, IProductService, IOrderService,
        + Implementations     IFieldService, IInventoryService, IDocumentService
Pass 4: ViewModels            Auth → Products → Orders → Field → Inventory → Documents → Users
Pass 5: Views + AppShell      Same domain order as ViewModels; update AppShell.xaml + App.xaml.cs
Pass 6: MauiProgram.cs        All DI registrations
Pass 7: Invoke reviewer       maui-project-reviewer
```

---

## Cynapharm Implementation Patterns

### 1. ResponseDto — `Models/Common/ResponseDto.cs`
```csharp
namespace Cynapharm_Mobile;

public class ResponseDto
{
    public object? Result { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string>? Errors { get; set; }
}
```

### 2. PagedResult — `Models/Common/PagedResult.cs`
```csharp
namespace Cynapharm_Mobile;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
}
```

### 3. ITokenManager + TokenManager — `Services/Token/`
```csharp
namespace Cynapharm_Mobile;

public interface ITokenManager
{
    Task SaveTokenAsync(string token);
    Task<string?> GetTokenAsync();
    Task RemoveTokenAsync();
    Task<bool> HasValidTokenAsync();
    Task<int?> GetUserIdAsync();
    Task<string?> GetUserRoleAsync();
}

public class TokenManager : ITokenManager
{
    private const string TokenKey = "jwt_token";

    public Task SaveTokenAsync(string token) => SecureStorage.SetAsync(TokenKey, token);
    public Task<string?> GetTokenAsync() => SecureStorage.GetAsync(TokenKey);
    public Task RemoveTokenAsync() { SecureStorage.Remove(TokenKey); return Task.CompletedTask; }

    public async Task<bool> HasValidTokenAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return false;
        try
        {
            var payload = DecodePayload(token);
            using var doc = System.Text.Json.JsonDocument.Parse(payload);
            var exp = doc.RootElement.GetProperty("exp").GetInt64();
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() < exp;
        }
        catch { return false; }
    }

    public async Task<int?> GetUserIdAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;
        var json = DecodePayload(token);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("sub", out var sub) && int.TryParse(sub.GetString(), out var id))
            return id;
        return null;
    }

    public async Task<string?> GetUserRoleAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;
        var json = DecodePayload(token);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("role", out var role) ? role.GetString() : null;
    }

    private static string DecodePayload(string token)
    {
        var part = token.Split('.')[1];
        var padded = part.PadRight(part.Length + (4 - part.Length % 4) % 4, '=');
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
```

### 4. JwtAuthHandler — `Services/Base/JwtAuthHandler.cs`
```csharp
namespace Cynapharm_Mobile;

public class JwtAuthHandler : DelegatingHandler
{
    private readonly ITokenManager _tokenManager;
    public JwtAuthHandler(ITokenManager tokenManager) => _tokenManager = tokenManager;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 5. ApiServiceBase — `Services/Base/ApiServiceBase.cs`
```csharp
namespace Cynapharm_Mobile;

public abstract class ApiServiceBase
{
    private readonly IHttpClientFactory _factory;
    protected const string ClientName = "CynapharmApi";
    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    protected ApiServiceBase(IHttpClientFactory factory) => _factory = factory;

    protected HttpClient CreateClient() => _factory.CreateClient(ClientName);

    protected async Task<T?> GetAsync<T>(string url)
    {
        var resp = await CreateClient().GetAsync(url);
        resp.EnsureSuccessStatusCode();
        var dto = await Deserialize<ResponseDto>(resp);
        return dto?.IsSuccess == true && dto.Result is not null
            ? System.Text.Json.JsonSerializer.Deserialize<T>(dto.Result.ToString()!, JsonOpts)
            : default;
    }

    protected async Task<ResponseDto?> PostAsync<TBody>(string url, TBody body)
    {
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(body),
            System.Text.Encoding.UTF8, "application/json");
        var resp = await CreateClient().PostAsync(url, content);
        return await Deserialize<ResponseDto>(resp);
    }

    protected async Task<ResponseDto?> PutAsync<TBody>(string url, TBody body)
    {
        var content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(body),
            System.Text.Encoding.UTF8, "application/json");
        var resp = await CreateClient().PutAsync(url, content);
        return await Deserialize<ResponseDto>(resp);
    }

    protected async Task<ResponseDto?> PutAsync(string url)
    {
        var resp = await CreateClient().PutAsync(url, null);
        return await Deserialize<ResponseDto>(resp);
    }

    protected async Task<ResponseDto?> DeleteAsync(string url)
    {
        var resp = await CreateClient().DeleteAsync(url);
        return await Deserialize<ResponseDto>(resp);
    }

    private static async Task<T?> Deserialize<T>(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, JsonOpts);
    }
}
```

### 6. BaseViewModel — `ViewModels/Base/BaseViewModel.cs`
```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cynapharm_Mobile;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? userRole;

    public bool IsNotBusy => !IsBusy;

    protected void SetError(string? message) => ErrorMessage = message;
    protected void ClearError() => ErrorMessage = null;
}
```

### 7. IAuthService Interface — `Services/Auth/IAuthService.cs`
```csharp
namespace Cynapharm_Mobile;

public interface IAuthService
{
    Task<(bool Success, string? Token, string? Message)> LoginAsync(string username, string password);
    Task<ResponseDto?> ForgotPasswordAsync(string email);
    Task<ResponseDto?> ChangePasswordAsync(string email, string currentPassword, string newPassword);
    Task<List<UserDto>?> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<List<UserDto>?> SearchUsersAsync(string keyword);
    Task<ResponseDto?> RegisterUserAsync(RegistrationRequestDto request);
    Task<ResponseDto?> ChangeRoleAsync(string email, string newRole);
    Task<ResponseDto?> EnableUserAsync(string email);
    Task<ResponseDto?> DisableUserAsync(string email);
}
```

### 8. AuthService Login Pattern — `Services/Auth/AuthService.cs`
```csharp
public async Task<(bool Success, string? Token, string? Message)> LoginAsync(string username, string password)
{
    var result = await PostAsync("auth/login", new LoginRequestDto { UserName = username, Password = password });
    if (result?.IsSuccess == true && result.Result is not null)
    {
        var loginResponse = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(
            result.Result.ToString()!,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return (true, loginResponse?.Token, null);
    }
    return (false, null, result?.Message ?? "Login failed");
}
```

### 9. LoginViewModel — `ViewModels/Auth/LoginViewModel.cs`
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cynapharm_Mobile;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly ITokenManager _tokenManager;

    public LoginViewModel(IAuthService authService, ITokenManager tokenManager)
    {
        _authService = authService;
        _tokenManager = tokenManager;
    }

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        ClearError();
        try
        {
            var (success, token, message) = await _authService.LoginAsync(Username, Password);
            if (success && token is not null)
            {
                await _tokenManager.SaveTokenAsync(token);
                await Shell.Current.GoToAsync("//products");
            }
            else
            {
                SetError(message);
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

### 10. View Code-Behind Pattern — `Views/[Domain]/FeaturePage.xaml.cs`
```csharp
namespace Cynapharm_Mobile;

public partial class ProductListPage : ContentPage
{
    public ProductListPage(ProductListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

### 11. App.xaml.cs — Conditional Navigation on Start
```csharp
protected override Window CreateWindow(IActivationState? activationState)
{
    var tokenManager = Handler!.MauiContext!.Services.GetRequiredService<ITokenManager>();
    var hasToken = tokenManager.HasValidTokenAsync().GetAwaiter().GetResult();
    return new Window(hasToken ? new AppShell() : new NavigationPage(new LoginPage(
        Handler.MauiContext.Services.GetRequiredService<LoginViewModel>())));
}
```

### 12. MauiProgram.cs DI Registration Order
```csharp
// 1. Singleton — TokenManager lives for app lifetime
builder.Services.AddSingleton<ITokenManager, TokenManager>();

// 2. Transient — new handler per HttpClient pipeline
builder.Services.AddTransient<JwtAuthHandler>();

// 3. Named HttpClient with JWT injection
builder.Services.AddHttpClient("CynapharmApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7099/");
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<JwtAuthHandler>();

// 4. Services — Transient (new per navigation/page)
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IFieldService, FieldService>();
builder.Services.AddTransient<IInventoryService, InventoryService>();
builder.Services.AddTransient<IDocumentService, DocumentService>();

// 5. ViewModels — Transient
// (register all 21 ViewModels here)

// 6. Pages — Transient (required for constructor injection to work)
// (register all 21 Pages here)
```

---

## Implementation Constraints

- After implementing each service, verify the endpoint path matches Gateway upstream paths exactly
- Do NOT use `HttpClient` directly in ViewModels — always inject `IXxxService`
- Do NOT use `SecureStorage` outside of `TokenManager`
- All service method return types must be `Task<T?>` or `Task<ResponseDto?>` — never `void`
- Always check `dto.IsSuccess` before unwrapping `dto.Result` in service methods
- Every async `[RelayCommand]` method must set `IsBusy = true` at start and reset in `finally`
- When adding new features, ALWAYS follow the order: Model → Interface → Service → ViewModel → View → Register in MauiProgram.cs → Add Shell route

> This subagent is internal to the orchestrator and should not be invoked directly by users.
