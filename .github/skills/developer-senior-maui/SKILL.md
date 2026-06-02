  ---
  name: developer-senior-maui
  description: "Skill for the developer subagent to implement .NET MAUI applications as a senior MAUI developer."
  ---

  # Developer Senior MAUI Skill

  ## Purpose
  This skill enables the developer subagent to act as a senior .NET MAUI developer, implementing mobile and desktop application features using MVVM, Shell navigation, DI, and platform-aware services.

  ## Implementation Requirements
  The MAUI application MUST follow these structure and design principles:
  - Views in `Views/` with paired XAML and code-behind files:
    - `FeaturePage.xaml`
    - `FeaturePage.xaml.cs`
  - View models in `ViewModels/` with `ViewModel` suffix:
    - `FeatureViewModel.cs`
  - Services in `Services/` for API clients, data access, and platform-specific features.
  - Models in `Models/` for domain and API DTO objects.
  - Resources in `Resources/` for styles, fonts, and images.
  - Register services and view models in `MauiProgram.cs` using dependency injection.
  - Use `AppShell.xaml` for global navigation and shell routes.
  - Keep XAML layout separate from logic and keep code-behind minimal.
  - Bind views to view models via `BindingContext` and use commands for actions.

  ## Workflow
  1. **Review the plan**: Understand the approved mobile/MAUI architecture plan.
  2. **Create project structure**: Generate `Views/`, `ViewModels/`, `Services/`, `Models/`, and `Resources/` as required.
  3. **Implement views**: Build XAML pages with clear layout, data bindings, and UI structure.
  4. **Implement view models**: Add properties, commands, and service interactions in MVVM-friendly view models.
  5. **Implement services**: Build backend API clients, data services, and any platform-specific service wrappers.
  6. **Connect DI and navigation**: Register services and view models in `MauiProgram.cs` and configure shell routes in `AppShell.xaml`.
  7. **Validate and refine**: Fix compile or binding issues and ensure the app follows the plan.

  ## Output
  - A .NET MAUI implementation with structured `Views/`, `ViewModels/`, `Services/`, `Models/`, and `Resources/`.
  - XAML-based pages wired to view models.
  - Proper dependency injection registration.
  - AppShell navigation and routing configured.
  - Maintainable MVVM application structure.
