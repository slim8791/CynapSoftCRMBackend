# Analyse de l'utilisation de CommunityToolkit.MVVM dans Cynapharm Mobile

> **Projet** : Cynapharm-Mobile — Application .NET MAUI  
> **Librairie analysée** : `CommunityToolkit.Mvvm` version `8.*`  
> **Nombre de ViewModels** : 25  
> **Date** : 2026-05-31

---

## Table des matières

1. [Introduction à CommunityToolkit.MVVM](#1-introduction-à-communitytoolkitmvvm)
2. [ObservableProperty — Propriétés réactives](#2-observableproperty--propriétés-réactives)
3. [RelayCommand — Commandes bindables](#3-relaycommand--commandes-bindables)
4. [NotifyPropertyChangedFor — Chaînes de notification](#4-notifypropertychangedfor--chaînes-de-notification)
5. [NotifyDataErrorInfo — Validation de formulaires](#5-notifydataerrorinfo--validation-de-formulaires)
6. [ObservableValidator — Base de validation](#6-observablevalidator--base-de-validation)
7. [Partial Methods — Réaction aux changements de propriétés](#7-partial-methods--réaction-aux-changements-de-propriétés)
8. [QueryProperty — Navigation avec paramètres](#8-queryproperty--navigation-avec-paramètres)
9. [WeakReferenceMessenger — Communication entre ViewModels](#9-weakreferencemessenger--communication-entre-viewmodels)
10. [Bilan et bénéfices dans le projet](#10-bilan-et-bénéfices-dans-le-projet)

---

## 1. Introduction à CommunityToolkit.MVVM

### Qu'est-ce que le pattern MVVM ?

**MVVM** (Model-View-ViewModel) est un patron d'architecture qui sépare l'interface utilisateur (View) de la logique métier (ViewModel) et des données (Model).

```
┌────────────┐     Binding     ┌──────────────────┐     API     ┌──────────┐
│    View    │ ◄─────────────► │   ViewModel      │ ──────────► │  Model   │
│  (XAML)   │                  │  (C# class)      │             │  (Data)  │
└────────────┘                  └──────────────────┘             └──────────┘
```

Sans outillage, implémenter MVVM en C# nécessite beaucoup de code répétitif :

```csharp
// ❌ MVVM traditionnel — beaucoup de code "boilerplate"
private string _email;
public string Email
{
    get => _email;
    set
    {
        if (_email == value) return;
        _email = value;
        OnPropertyChanged(nameof(Email)); // notification manuelle
    }
}

private ICommand _loginCommand;
public ICommand LoginCommand => _loginCommand ??= new Command(async () => await LoginAsync());
```

### Ce que CommunityToolkit.MVVM apporte

**CommunityToolkit.MVVM** est une bibliothèque officielle Microsoft qui utilise la **génération de code source** (Source Generators) pour éliminer ce boilerplate. Au lieu d'écrire la propriété complète, il suffit d'ajouter un attribut :

```csharp
// ✅ Avec CommunityToolkit.MVVM — une seule ligne
[ObservableProperty]
private string _email = string.Empty;
```

Le compilateur génère automatiquement la propriété publique `Email` avec `OnPropertyChanged()`.

### Installation dans le projet

Dans [Cynapharm-Mobile.csproj](../Cynapharm-Mobile.csproj) :

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />
```

---

## 2. `[ObservableProperty]` — Propriétés réactives

### Concept

`[ObservableProperty]` est placé sur un **champ privé** (field). Le Toolkit génère automatiquement :
- Une **propriété publique** (ex: `_email` → `Email`)
- L'appel à `OnPropertyChanged()` à chaque modification
- Des méthodes partielles `OnXxxChanging` / `OnXxxChanged` pour des actions supplémentaires

### Exemple 1 — `LoginViewModel` : Propriétés simples

**Fichier** : [ViewModels/Auth/LoginViewModel.cs](../ViewModels/Auth/LoginViewModel.cs)

```csharp
public partial class LoginViewModel : BaseViewModel
{
    [ObservableProperty] private string _email           = string.Empty;
    [ObservableProperty] private string _password        = string.Empty;
    [ObservableProperty] private bool   _isPasswordHidden = true;
}
```

**Liaison XAML correspondante :**

```xml
<Entry Text="{Binding Email}" Placeholder="Email" />
<Entry Text="{Binding Password}" IsPassword="{Binding IsPasswordHidden}" />
```

---

✅ **But du code**  
Ces trois champs représentent l'état du formulaire de connexion. Dès que l'utilisateur tape dans un `Entry`, la propriété se met à jour et notifie automatiquement l'interface.

⚙️ **Comment ça fonctionne en interne**  
Le Toolkit génère le code suivant (invisible, dans un fichier auto-généré) :

```csharp
// Code généré automatiquement — vous ne l'écrivez jamais
public string Email
{
    get => _email;
    set
    {
        if (!EqualityComparer<string>.Default.Equals(_email, value))
        {
            OnEmailChanging(value);
            OnPropertyChanging(nameof(Email));
            _email = value;
            OnEmailChanged(value);
            OnPropertyChanged(nameof(Email));
        }
    }
}
```

🔥 **Pourquoi c'est utile dans l'application**  
Avec 25 ViewModels et plus de 120 propriétés observables, écrire chaque propriété manuellement aurait nécessité des centaines de lignes supplémentaires. Le Toolkit réduit chaque propriété à **une seule ligne**.

⚠️ **Bonne pratique**  
- Toujours nommer les champs avec le préfixe `_` (ex : `_email` → génère `Email`)
- La classe doit être `partial` pour que la génération de code fonctionne
- Ne pas oublier d'hériter de `ObservableObject` (ou `ObservableValidator`)

---

### Exemple 2 — `DashboardViewModel` : Propriétés de types variés

**Fichier** : [ViewModels/Dashboard/DashboardViewModel.cs](../ViewModels/Dashboard/DashboardViewModel.cs)

```csharp
public partial class DashboardViewModel : BaseViewModel
{
    [ObservableProperty] private string           _userDisplayName  = string.Empty;
    [ObservableProperty] private string           _userRole         = string.Empty;
    [ObservableProperty] private string           _greetingInitials = "?";
    [ObservableProperty] private int              _todayVisitCount;
    [ObservableProperty] private bool             _isSuperviseur;
    [ObservableProperty] private bool             _isDelegue;
    [ObservableProperty] private double           _tauxConversion;
    [ObservableProperty] private StockSummaryDto? _stockSummary;
}
```

---

✅ **But du code**  
Le tableau de bord affiche des informations différentes selon le rôle de l'utilisateur. Ces propriétés alimentent tous les éléments visuels de la page d'accueil.

⚙️ **Comment ça fonctionne en interne**  
`[ObservableProperty]` fonctionne avec **n'importe quel type** C# : `string`, `int`, `bool`, `double`, objets complexes (`StockSummaryDto?`). La notification `OnPropertyChanged` est générée pour chaque type.

🔥 **Pourquoi c'est utile dans l'application**  
Le tableau de bord adapte son affichage en temps réel : le nombre de visites du jour (`TodayVisitCount`), les initiales de l'utilisateur (`GreetingInitials`), le taux de conversion (`TauxConversion`) se mettent à jour dès que les données arrivent de l'API.

⚠️ **Bonne pratique**  
- Pour les types nullables (`StockSummaryDto?`), l'interface doit utiliser `IsVisible="{Binding StockSummary, Converter={StaticResource IsNotNullConverter}}"` pour éviter les NullReferenceException.

---

### Exemple 3 — `BaseViewModel` : Propriétés transversales

**Fichier** : [ViewModels/Base/BaseViewModel.cs](../ViewModels/Base/BaseViewModel.cs)

```csharp
public partial class BaseViewModel : ObservableValidator
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isOffline;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
}
```

---

✅ **But du code**  
`BaseViewModel` définit les propriétés communes à **tous les 25 ViewModels** : état de chargement (`IsBusy`), état de rafraîchissement (`IsRefreshing`), message d'erreur (`ErrorMessage`), mode hors ligne (`IsOffline`). Chaque ViewModel hérite automatiquement de ces capacités.

⚙️ **Comment ça fonctionne en interne**  
Grâce à l'héritage, tous les ViewModels disposent de `IsBusy`, `ErrorMessage`, etc. La XAML peut lier `{Binding IsBusy}` et `{Binding ErrorMessage}` sur n'importe quelle page sans code supplémentaire.

🔥 **Pourquoi c'est utile dans l'application**  
Un seul `ActivityIndicator` lié à `{Binding IsBusy}` suffit pour afficher un spinner de chargement sur toutes les pages de l'app. C'est la puissance de la centralisation MVVM.

⚠️ **Bonne pratique**  
- `IsBusy` doit toujours être géré via les méthodes `ExecuteAsync` / `SetBusy` du `BaseViewModel` pour éviter les états incohérents.

---

## 3. `[RelayCommand]` — Commandes bindables

### Concept

`[RelayCommand]` est placé sur une **méthode privée**. Le Toolkit génère une propriété publique `XxxCommand` qui implémente `ICommand`, utilisable directement dans les bindings XAML.

```csharp
// ❌ Traditionnel — 5 lignes par commande
private ICommand _loginCommand;
public ICommand LoginCommand =>
    _loginCommand ??= new AsyncRelayCommand(LoginAsync);

private async Task LoginAsync() { ... }
```

```csharp
// ✅ Avec le Toolkit — 1 attribut
[RelayCommand]
private async Task LoginAsync() { ... }
// → génère automatiquement : public IAsyncRelayCommand LoginAsyncCommand
```

### Exemple 1 — `LoginViewModel` : Commandes synchrones et asynchrones

**Fichier** : [ViewModels/Auth/LoginViewModel.cs](../ViewModels/Auth/LoginViewModel.cs)

```csharp
[RelayCommand]
private Task LoginAsync() => ExecuteAsync(async () =>
{
    if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
    {
        ErrorMessage = "Veuillez renseigner votre email et mot de passe.";
        return;
    }
    if (!await CheckConnectivityAsync()) return;

    var result = await _authService.LoginAsync(new LoginRequest(Email, Password));

    var role = result.User.Role;
    var target = role switch
    {
        "DELEGUE" or "ADMIN" or "SUPERVISEUR" => "//dashboard",
        "PHARMACIEN" or "GROSSISTE" or "CLIENT" => "//orders",
        "MEDECIN" => "//products",
        _ => "//orders"
    };
    await Shell.Current.GoToAsync(target);
});

[RelayCommand]
private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;

[RelayCommand]
private async Task GoToForgotPasswordAsync()
    => await Shell.Current.GoToAsync("forgotpassword");
```

**Liaison XAML correspondante :**

```xml
<Button Text="Se connecter" Command="{Binding LoginAsyncCommand}" />
<ImageButton Command="{Binding TogglePasswordVisibilityCommand}" />
<Label Text="Mot de passe oublié ?" >
    <Label.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding GoToForgotPasswordAsyncCommand}" />
    </Label.GestureRecognizers>
</Label>
```

---

✅ **But du code**  
- `LoginAsync` : valide les champs, appelle l'API d'authentification, puis redirige vers la bonne page selon le rôle de l'utilisateur.  
- `TogglePasswordVisibility` : bascule l'affichage du mot de passe (masqué ↔ visible).  
- `GoToForgotPasswordAsync` : navigue vers la page de récupération de mot de passe.

⚙️ **Comment ça fonctionne en interne**  
Le Toolkit génère trois propriétés publiques :
```csharp
// Code généré automatiquement
public IAsyncRelayCommand LoginAsyncCommand { get; }
public IRelayCommand TogglePasswordVisibilityCommand { get; }
public IAsyncRelayCommand GoToForgotPasswordAsyncCommand { get; }
```
Pour les méthodes `async Task`, la commande générée est `IAsyncRelayCommand` qui gère automatiquement l'état en cours d'exécution.

🔥 **Pourquoi c'est utile dans l'application**  
La page de connexion a 3 actions distinctes. Sans le Toolkit, il aurait fallu déclarer 3 propriétés `ICommand` séparément. Ici, chaque action est simplement une méthode C# normale avec un attribut.

⚠️ **Bonne pratique**  
- Pour les commandes asynchrones, le nom de la méthode doit se terminer par `Async` (ex: `LoginAsync`) : le Toolkit génère la commande sous le nom `LoginAsyncCommand`.  
- Pour les commandes synchrones (ex: `TogglePasswordVisibility`), la commande générée s'appelle `TogglePasswordVisibilityCommand`.

---

### Exemple 2 — `DashboardViewModel` : Commandes de navigation

**Fichier** : [ViewModels/Dashboard/DashboardViewModel.cs](../ViewModels/Dashboard/DashboardViewModel.cs)

```csharp
[RelayCommand]
private async Task LoadDashboardAsync()
{
    ClearError();
    SetBusy(true);
    await InitializeAsync();
    if (!await CheckConnectivityAsync())
    {
        await LoadFromCacheAsync();
        SetBusy(false);
        return;
    }

    try
    {
        var today = DateTime.Today;
        // Chargement en parallèle de plusieurs APIs
        var perfTask = _kpiService.GetPerformanceAsync(monthStart, today);
        var objTask  = _kpiService.GetObjectifsAsync();
        await Task.WhenAll(perfTask, objTask);

        TodayVisitCount = visites?.Count ?? 0;
        TauxConversion  = taux ?? 0;
    }
    catch (Exception) { await LoadFromCacheAsync(); }
    finally { SetBusy(false); }
}

[RelayCommand]
private async Task GoToVisitsAsync()    => await Shell.Current.GoToAsync("//visits");

[RelayCommand]
private async Task GoToPlanningAsync()  => await Shell.Current.GoToAsync("//planning");

[RelayCommand]
private async Task GoToObjectifsAsync() => await Shell.Current.GoToAsync("//objectifs");
```

---

✅ **But du code**  
`LoadDashboardAsync` charge toutes les données de la page d'accueil depuis l'API (ou le cache si hors ligne) et redirige vers la bonne section selon le rôle. Les trois commandes `GoTo...` permettent la navigation vers les sections de l'application.

⚙️ **Comment ça fonctionne en interne**  
`[RelayCommand]` sur une méthode `async Task` génère un `AsyncRelayCommand`. Cette commande est **thread-safe** et désactive automatiquement le bouton pendant l'exécution pour éviter les double-clics.

🔥 **Pourquoi c'est utile dans l'application**  
Le dashboard est la page centrale de l'app. Elle doit charger des données depuis plusieurs services API en parallèle (`Task.WhenAll`). `[RelayCommand]` permet d'exposer cette logique complexe en une simple commande XAML.

⚠️ **Bonne pratique**  
- Toujours appeler `SetBusy(true)` en début de commande et `SetBusy(false)` dans le bloc `finally` pour garantir que le spinner disparaît même en cas d'erreur.

---

### Exemple 3 — `MesClientsViewModel` : Commande avec paramètre

**Fichier** : [ViewModels/Clients/MesClientsViewModel.cs](../ViewModels/Clients/MesClientsViewModel.cs)

```csharp
[RelayCommand]
private async Task GoToDetailAsync(UserListItem client)
{
    if (client == null) return;
    await Shell.Current.GoToAsync($"///clients/detail?clientId={client.Id}");
}

[RelayCommand]
private async Task CreateClientAsync()
{
    await Shell.Current.GoToAsync("///clients/form");
}
```

**Liaison XAML correspondante :**

```xml
<CollectionView ItemsSource="{Binding Clients}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid>
                <Grid.GestureRecognizers>
                    <TapGestureRecognizer
                        Command="{Binding Source={RelativeSource AncestorType={x:Type vm:MesClientsViewModel}},
                                          Path=GoToDetailAsyncCommand}"
                        CommandParameter="{Binding .}" />
                </Grid.GestureRecognizers>
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

---

✅ **But du code**  
`GoToDetailAsync` reçoit l'objet `UserListItem` du client sélectionné dans la liste et navigue vers la page de détail en passant son identifiant en paramètre de route Shell.

⚙️ **Comment ça fonctionne en interne**  
Le Toolkit génère `GoToDetailAsyncCommand` qui est un `IAsyncRelayCommand<UserListItem>` — il accepte un paramètre typé. En XAML, `CommandParameter="{Binding .}"` passe l'objet courant de la `DataTemplate`.

🔥 **Pourquoi c'est utile dans l'application**  
Dans une liste de clients, chaque élément doit permettre la navigation vers son détail. Le paramètre de commande évite de créer une commande séparée par client ou de stocker une propriété `SelectedClient` intermédiaire.

⚠️ **Bonne pratique**  
- Toujours vérifier `if (client == null) return;` car MAUI peut appeler la commande avec `null` dans certains cas de cycle de vie.

---

### Exemple 4 — `ChangePasswordViewModel` : Commandes de bascule d'état

**Fichier** : [ViewModels/Profile/ChangePasswordViewModel.cs](../ViewModels/Profile/ChangePasswordViewModel.cs)

```csharp
[RelayCommand]
private void ToggleShowOldPassword()     => ShowOldPassword     = !ShowOldPassword;

[RelayCommand]
private void ToggleShowNewPassword()     => ShowNewPassword     = !ShowNewPassword;

[RelayCommand]
private void ToggleShowConfirmPassword() => ShowConfirmPassword = !ShowConfirmPassword;
```

---

✅ **But du code**  
Ces trois commandes contrôlent la visibilité des caractères dans les champs de mot de passe (l'icône "œil" que l'utilisateur clique pour voir/masquer le mot de passe).

⚙️ **Comment ça fonctionne en interne**  
Les commandes synchrones (`void`) génèrent `IRelayCommand` (et non `IAsyncRelayCommand`). L'exécution est immédiate, sans état de chargement.

🔥 **Pourquoi c'est utile dans l'application**  
Trois boutons, trois commandes en trois lignes. Sans le Toolkit, il aurait fallu déclarer trois propriétés `ICommand` avec chacune un lambda ou une méthode.

⚠️ **Bonne pratique**  
- Préférer `[RelayCommand]` sur une méthode `void` plutôt que `async Task` pour les actions synchrones instantanées — cela évite de créer inutilement un `AsyncRelayCommand`.

---

## 4. `[NotifyPropertyChangedFor]` — Chaînes de notification

### Concept

Quand une propriété change, d'autres propriétés **calculées** qui en dépendent doivent aussi notifier l'interface. `[NotifyPropertyChangedFor]` enchaîne automatiquement ces notifications.

```csharp
// ❌ Traditionnel — notification manuelle dans le setter
set
{
    _stocksFaibles = value;
    OnPropertyChanged(nameof(StocksFaibles));
    OnPropertyChanged(nameof(HasStockFaible));  // doit être notifiée aussi !
    OnPropertyChanged(nameof(StockFaibleLabel));  // et celle-là aussi !
}
```

```csharp
// ✅ Avec le Toolkit
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(HasStockFaible), nameof(StockFaibleLabel))]
private int _stocksFaibles = 0;
```

### Exemple 1 — `DashboardViewModel` : Alertes de stock

**Fichier** : [ViewModels/Dashboard/DashboardViewModel.cs](../ViewModels/Dashboard/DashboardViewModel.cs)

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(HasStockFaible), nameof(StockFaibleLabel))]
private int _stocksFaibles = 0;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(HasRuptureStock), nameof(RuptureStockLabel))]
private int _stocksVides = 0;

public bool   HasStockFaible    => StocksFaibles > 0;
public bool   HasRuptureStock   => StocksVides   > 0;
public string StockFaibleLabel  => $"⚠️ {StocksFaibles} produit(s) en stock faible";
public string RuptureStockLabel => $"⚠️ {StocksVides} produit(s) en rupture de stock";
```

---

✅ **But du code**  
Quand le nombre de stocks faibles (`StocksFaibles`) change (ex: chargé depuis l'API), la bannière d'alerte apparaît ou disparaît automatiquement (`HasStockFaible`) et son texte se met à jour (`StockFaibleLabel`).

⚙️ **Comment ça fonctionne en interne**  
Le setter généré pour `StocksFaibles` appelle automatiquement :
```csharp
OnPropertyChanged(nameof(StocksFaibles));
OnPropertyChanged(nameof(HasStockFaible));   // ← grâce à [NotifyPropertyChangedFor]
OnPropertyChanged(nameof(StockFaibleLabel)); // ← grâce à [NotifyPropertyChangedFor]
```

🔥 **Pourquoi c'est utile dans l'application**  
Le tableau de bord du délégué médical doit afficher des alertes visuelles en temps réel sur l'état de son stock. Cette cascade de notifications garantit que l'UI est toujours cohérente avec les données.

⚠️ **Bonne pratique**  
- Les propriétés calculées (`HasStockFaible`, `StockFaibleLabel`) n'ont **pas** besoin de `[ObservableProperty]` car elles ne stockent pas de valeur — elles se **calculent** depuis d'autres propriétés.

---

### Exemple 2 — `VisitDetailViewModel` : État d'une visite

**Fichier** : [ViewModels/Visites/VisitDetailViewModel.cs](../ViewModels/Visites/VisitDetailViewModel.cs)

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CanEdit))]
[NotifyPropertyChangedFor(nameof(CanDelete))]
[NotifyPropertyChangedFor(nameof(ShowEditRapport))]
[NotifyPropertyChangedFor(nameof(ShowViewRapport))]
[NotifyPropertyChangedFor(nameof(CanStartVisite))]
private bool _isCompleted;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(ShowSubmitRapport))]
[NotifyPropertyChangedFor(nameof(ShowEditRapport))]
[NotifyPropertyChangedFor(nameof(ShowViewRapport))]
private bool _hasRapport;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CanStartVisite))]
[NotifyPropertyChangedFor(nameof(ShowSubmitRapport))]
[NotifyPropertyChangedFor(nameof(ShowEditRapport))]
private bool _isStarted;

public bool CanStartVisite    => IsExisting && !IsStarted && !IsCompleted;
public bool CanEdit           => !IsCompleted;
public bool CanDelete         => IsExisting && !IsCompleted;
public bool ShowSubmitRapport => IsExisting && !HasRapport && IsStarted;
public bool ShowEditRapport   => IsExisting && HasRapport && !IsCompleted;
public bool ShowViewRapport   => IsExisting && HasRapport && IsCompleted;
```

---

✅ **But du code**  
La page de détail d'une visite affiche des boutons différents selon l'état de la visite. Quand `IsCompleted` passe à `true` (validée par le superviseur), le bouton "Modifier" disparaît, le bouton "Voir le rapport" apparaît, et les champs du formulaire se verrouillent — tout automatiquement.

⚙️ **Comment ça fonctionne en interne**  
Trois propriétés booléennes (`IsCompleted`, `HasRapport`, `IsStarted`) contrôlent **5 à 6 propriétés dérivées** chacune. Sans `[NotifyPropertyChangedFor]`, il faudrait 15+ appels `OnPropertyChanged` manuels dans les setters.

🔥 **Pourquoi c'est utile dans l'application**  
Le workflow d'une visite médicale passe par plusieurs états (créée → démarrée → rapport soumis → validée). Chaque transition d'état doit mettre à jour l'UI de façon cohérente. Ce mécanisme garantit qu'aucun bouton n'est affiché au mauvais moment.

⚠️ **Bonne pratique**  
- Quand une propriété doit notifier **de nombreuses** propriétés dérivées, envisager de grouper la notification dans une `partial void OnXxxChanged()` qui appelle `OnPropertyChanged` sur plusieurs propriétés à la fois — plus lisible.

---

### Exemple 3 — `ChangePasswordViewModel` : Indicateur de force du mot de passe

**Fichier** : [ViewModels/Profile/ChangePasswordViewModel.cs](../ViewModels/Profile/ChangePasswordViewModel.cs)

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(PasswordStrength), nameof(StrengthLabel),
    nameof(Seg1Color), nameof(Seg2Color), nameof(Seg3Color), nameof(Seg4Color),
    nameof(PasswordsMatch), nameof(PasswordsMismatch))]
private string _newPassword = string.Empty;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(PasswordsMatch), nameof(PasswordsMismatch))]
private string _confirmPassword = string.Empty;

public int    PasswordStrength => Strength(NewPassword);
public string StrengthLabel    => PasswordStrength switch { 1 => "Faible", 2 => "Moyen", 3 => "Fort", 4 => "Très fort", _ => string.Empty };
public Color  Seg1Color        => PasswordStrength >= 1 ? StrengthColor(1) : Color.FromArgb("#E0E0E0");
// ... Seg2Color, Seg3Color, Seg4Color
public bool   PasswordsMatch   => NewPassword == ConfirmPassword && !string.IsNullOrEmpty(NewPassword);
public bool   PasswordsMismatch => NewPassword != ConfirmPassword && !string.IsNullOrEmpty(NewPassword);
```

---

✅ **But du code**  
À chaque frappe dans le champ "Nouveau mot de passe", l'indicateur de force (4 segments de couleur), le label ("Faible", "Fort"...) et l'indicateur de correspondance se mettent à jour en temps réel.

⚙️ **Comment ça fonctionne en interne**  
`_newPassword` notifie **8 propriétés** en cascade. Une seule modification de champ déclenche la mise à jour complète de tous les indicateurs visuels.

🔥 **Pourquoi c'est utile dans l'application**  
L'UX du changement de mot de passe est enrichie sans complexifier le code : l'utilisateur reçoit un retour immédiat sur la force et la correspondance de son mot de passe.

⚠️ **Bonne pratique**  
- Garder les propriétés calculées `pure` (sans effets de bord) pour que la notification soit fiable et prévisible.

---

## 5. `[NotifyDataErrorInfo]` — Validation de formulaires

### Concept

`[NotifyDataErrorInfo]` active la validation de champs avec les attributs standard de .NET (`[Required]`, `[Range]`, `[MinLength]`...). Les erreurs sont automatiquement propagées à l'interface via `INotifyDataErrorInfo`.

### Exemple 1 — `CreateOrderViewModel` : Validation de quantité

**Fichier** : [ViewModels/Orders/CreateOrderViewModel.cs](../ViewModels/Orders/CreateOrderViewModel.cs)

```csharp
[ObservableProperty]
[NotifyDataErrorInfo]
[Range(1, 9999, ErrorMessage = "La quantité doit être comprise entre 1 et 9 999.")]
[NotifyPropertyChangedFor(nameof(QuantityError))]
private int _quantity = 1;

public string QuantityError =>
    GetErrors(nameof(Quantity))
        .Cast<ValidationResult>()
        .FirstOrDefault()?.ErrorMessage ?? string.Empty;
```

---

✅ **But du code**  
Quand l'utilisateur saisit une quantité invalide (ex: 0 ou 10000), l'erreur "La quantité doit être comprise entre 1 et 9 999." s'affiche automatiquement sans que le formulaire soit soumis.

⚙️ **Comment ça fonctionne en interne**  
1. `[NotifyDataErrorInfo]` indique au Toolkit de valider le champ à chaque changement.
2. `[Range(1, 9999)]` définit la règle de validation.
3. `GetErrors(nameof(Quantity))` récupère le message d'erreur actuel.
4. `[NotifyPropertyChangedFor(nameof(QuantityError))]` met à jour l'affichage de l'erreur en temps réel.

🔥 **Pourquoi c'est utile dans l'application**  
La création de commande est un formulaire critique — une quantité invalide ne doit pas atteindre le backend. La validation côté client donne un retour immédiat à l'utilisateur.

⚠️ **Bonne pratique**  
- Toujours appeler `ValidateProperty(Quantity, nameof(Quantity))` avant une soumission pour forcer la validation si l'utilisateur n'a pas touché au champ.
- `[NotifyDataErrorInfo]` nécessite que la classe hérite de `ObservableValidator` (et non `ObservableObject`).

---

### Exemple 2 — `RapportViewModel` : Validation multi-attributs

**Fichier** : [ViewModels/Rapports/RapportViewModel.cs](../ViewModels/Rapports/RapportViewModel.cs)

```csharp
[ObservableProperty]
[NotifyDataErrorInfo]
[Required(ErrorMessage = "Le contenu du rapport est requis.")]
[MinLength(20, ErrorMessage = "Le rapport doit contenir au moins 20 caractères.")]
[NotifyPropertyChangedFor(nameof(ContenuError))]
[NotifyPropertyChangedFor(nameof(CanSubmit))]
private string _contenu = string.Empty;

public string ContenuError =>
    GetErrors(nameof(Contenu))
        .Cast<ValidationResult>()
        .FirstOrDefault()?.ErrorMessage ?? string.Empty;

public bool CanSubmit =>
    !HasErrors
    && !IsBusy
    && !IsReadOnly
    && CapturedLatitude.HasValue
    && CapturedLongitude.HasValue;
```

---

✅ **But du code**  
Le contenu du rapport de visite est obligatoire et doit faire au moins 20 caractères. `CanSubmit` combine la validation du formulaire, l'état de chargement, le mode lecture seule ET la capture GPS pour déterminer si le bouton "Soumettre" est actif.

⚙️ **Comment ça fonctionne en interne**  
Plusieurs attributs de validation peuvent être empilés sur la même propriété. Le Toolkit les évalue **tous** et remonte chaque erreur via `GetErrors()`. `CanSubmit` est une propriété calculée qui synthétise toutes ces conditions.

🔥 **Pourquoi c'est utile dans l'application**  
Un rapport de visite médicale est un document légal — sa validation est critique. Combiner `[Required]`, `[MinLength]` et la condition GPS dans `CanSubmit` garantit que le bouton de soumission n'est jamais actif si le formulaire est incomplet.

⚠️ **Bonne pratique**  
- Connecter `ErrorsChanged += (_, _) => OnPropertyChanged(nameof(CanSubmit))` dans le constructeur pour que `CanSubmit` se réévalue aussi quand les erreurs changent (pas seulement quand `_contenu` change).

---

## 6. `ObservableValidator` — Base de validation

### Concept

`ObservableValidator` est la classe de base à utiliser (à la place de `ObservableObject`) quand des validations `[NotifyDataErrorInfo]` sont nécessaires. Elle implémente `INotifyDataErrorInfo`.

**Fichier** : [ViewModels/Base/BaseViewModel.cs](../ViewModels/Base/BaseViewModel.cs)

```csharp
// ObservableValidator étend ObservableObject + ajoute INotifyDataErrorInfo
public partial class BaseViewModel : ObservableValidator
{
    // Tous les ViewModels héritent de BaseViewModel
    // → tous ont accès à la validation via [NotifyDataErrorInfo]
}
```

### Hiérarchie complète

```
ObservableObject          ← fourni par CommunityToolkit.Mvvm
    └── ObservableValidator   ← fourni par CommunityToolkit.Mvvm
            └── BaseViewModel     ← classe de base du projet
                    ├── LoginViewModel
                    ├── DashboardViewModel
                    ├── CreateOrderViewModel
                    ├── RapportViewModel
                    └── ... (21 autres ViewModels)
```

---

✅ **But du code**  
En faisant hériter `BaseViewModel` de `ObservableValidator`, tous les ViewModels du projet peuvent utiliser `[NotifyDataErrorInfo]` et `GetErrors()` sans configuration supplémentaire.

⚙️ **Comment ça fonctionne en interne**  
`ObservableValidator` maintient un dictionnaire interne des erreurs par propriété. `HasErrors` retourne `true` si au moins une propriété a une erreur. `ValidateAllProperties()` déclenche la validation sur tous les champs annotés.

🔥 **Pourquoi c'est utile dans l'application**  
Le projet Cynapharm contient plusieurs formulaires complexes (commandes, rapports de visite, changement de mot de passe). `ObservableValidator` centralise la mécanique de validation dans un seul endroit.

⚠️ **Bonne pratique**  
- Préférer `ObservableValidator` à `ObservableObject` dès qu'une classe utilise `[NotifyDataErrorInfo]`.
- Appeler `ValidateAllProperties()` avant toute soumission pour s'assurer que les champs non touchés sont aussi validés.

---

## 7. Partial Methods — Réaction aux changements de propriétés

### Concept

Quand `[ObservableProperty]` génère une propriété, il génère aussi deux méthodes partielles vides que le développeur peut **implémenter** pour réagir aux changements :

```csharp
partial void OnXxxChanging(T value); // appelée AVANT le changement
partial void OnXxxChanged(T value);  // appelée APRÈS le changement
```

### Exemple 1 — `MesClientsViewModel` : Filtre de recherche en temps réel

**Fichier** : [ViewModels/Clients/MesClientsViewModel.cs](../ViewModels/Clients/MesClientsViewModel.cs)

```csharp
[ObservableProperty]
private string _searchQuery = string.Empty;

// Appelée automatiquement chaque fois que SearchQuery change
partial void OnSearchQueryChanged(string value) => ApplyFilter();

private void ApplyFilter()
{
    Clients.Clear();
    var filtered = string.IsNullOrWhiteSpace(SearchQuery)
        ? _allClients
        : _allClients.Where(c =>
            (c.Name  ?? "").Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
            (c.Email ?? "").Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        .ToList();

    foreach (var c in filtered)
        Clients.Add(c);

    IsEmpty = Clients.Count == 0;
}
```

---

✅ **But du code**  
À chaque frappe dans la barre de recherche, la liste des clients est filtrée instantanément — sans bouton "Rechercher" à cliquer.

⚙️ **Comment ça fonctionne en interne**  
Le setter généré de `SearchQuery` appelle automatiquement `OnSearchQueryChanged` si la méthode est implémentée. Le développeur n'a qu'à implémenter `partial void OnSearchQueryChanged(string value)`.

🔥 **Pourquoi c'est utile dans l'application**  
La recherche en temps réel est une feature UX essentielle. Sans les méthodes partielles, il faudrait soit un `PropertyChanged` event souscrit dans le constructeur, soit une logique dans le setter manuellement écrit.

⚠️ **Bonne pratique**  
- Pour les recherches avec appels réseau, ajouter un **debounce** (délai avant d'exécuter la recherche) pour ne pas appeler l'API à chaque frappe. Voir `ProductListViewModel` pour un exemple avec `CancellationTokenSource`.

---

### Exemple 2 — `VisitDetailViewModel` : Réactions multiples aux changements

**Fichier** : [ViewModels/Visites/VisitDetailViewModel.cs](../ViewModels/Visites/VisitDetailViewModel.cs)

```csharp
partial void OnVisiteIdChanged(int value)
{
    Title = value > 0 ? "Détail visite" : "Nouvelle visite";
    OnPropertyChanged(nameof(IsNew));
    OnPropertyChanged(nameof(IsExisting));
    OnPropertyChanged(nameof(CanDelete));
    // ...
}

partial void OnPrefillDateChanged(string value)
{
    if (DateTime.TryParse(value, out var dt)) VisiteDate = dt;
}

partial void OnClientNameChanged(string value)    => _isDirty = true;
partial void OnNotesChanged(string value)         => _isDirty = true;
partial void OnSelectedTypeChanged(int value)
{
    _isDirty = true;
    OnPropertyChanged(nameof(SelectedTypeLabel));
}
```

---

✅ **But du code**  
- `OnVisiteIdChanged` : met à jour le titre de la page et les propriétés dérivées dès que l'identifiant de la visite est reçu via navigation.  
- `OnPrefillDateChanged` : convertit la date reçue en string (depuis Shell) en `DateTime`.  
- `OnClientNameChanged` / `OnNotesChanged` : marque le formulaire comme modifié (`_isDirty`) pour un éventuel dialog de confirmation avant de quitter.

⚙️ **Comment ça fonctionne en interne**  
Ces méthodes partielles sont appelées dans le setter généré, juste après que la valeur est affectée. Elles permettent des effets de bord sans polluer le setter.

🔥 **Pourquoi c'est utile dans l'application**  
La page de visite reçoit ses données via des paramètres de navigation Shell (strings). Les méthodes partielles permettent de convertir et réagir à ces changements de manière propre et découplée.

⚠️ **Bonne pratique**  
- Ne pas faire d'opérations lourdes (appels API) dans `OnXxxChanged` — préférer `OnXxxChanged` pour de la logique légère (conversion, flag, notification). Pour du chargement asynchrone, utiliser `OnXxxChanged` pour appeler `_ = LoadAsync()` avec discernement.

---

### Exemple 3 — `CreateOrderViewModel` : Pré-chargement de produit

**Fichier** : [ViewModels/Orders/CreateOrderViewModel.cs](../ViewModels/Orders/CreateOrderViewModel.cs)

```csharp
[ObservableProperty] private int _preselectedProductId;

partial void OnPreselectedProductIdChanged(int value)
{
    if (value > 0) _ = PreloadProductAsync(value);
}

private async Task PreloadProductAsync(int productId)
{
    var product = await _productService.GetProductByIdAsync(productId);
    if (product != null) SelectedProduct = product;
}
```

---

✅ **But du code**  
Quand l'utilisateur navigue vers "Créer une commande" depuis la page d'un produit, le produit est automatiquement pré-sélectionné dans le formulaire de commande.

⚙️ **Comment ça fonctionne en interne**  
`PreselectedProductId` est un `[QueryProperty]` : il est défini par la navigation Shell. Dès qu'il reçoit une valeur > 0, la méthode partielle déclenche le chargement du produit correspondant.

🔥 **Pourquoi c'est utile dans l'application**  
Cela crée un workflow fluide : "voir un produit → créer une commande avec ce produit" sans que l'utilisateur ait à rechercher à nouveau le produit.

---

## 8. `[QueryProperty]` — Navigation avec paramètres

### Concept

`[QueryProperty]` permet de recevoir des paramètres de navigation Shell directement comme propriétés de ViewModel, sans code d'interception manuel.

### Exemple 1 — `CreateOrderViewModel` : Paramètre de navigation

**Fichier** : [ViewModels/Orders/CreateOrderViewModel.cs](../ViewModels/Orders/CreateOrderViewModel.cs)

```csharp
[QueryProperty(nameof(PreselectedProductId), "productId")]
public partial class CreateOrderViewModel : BaseViewModel
{
    [ObservableProperty] private int _preselectedProductId;
    // ...
}
```

**Navigation depuis une autre page :**

```csharp
await Shell.Current.GoToAsync($"createorder?productId={product.Id}");
```

---

### Exemple 2 — `VisitDetailViewModel` : Plusieurs paramètres

**Fichier** : [ViewModels/Visites/VisitDetailViewModel.cs](../ViewModels/Visites/VisitDetailViewModel.cs)

```csharp
[QueryProperty(nameof(VisiteId),    "visiteId")]
[QueryProperty(nameof(PrefillDate), "prefillDate")]
[QueryProperty(nameof(IdPlanningRaw), "idPlanning")]
public partial class VisitDetailViewModel : BaseViewModel
{
    [ObservableProperty] private int    _visiteId;
    [ObservableProperty] private string _prefillDate  = string.Empty;
    [ObservableProperty] private string _idPlanningRaw = string.Empty;
}
```

**Navigation correspondante :**

```csharp
await Shell.Current.GoToAsync($"///visits/detail?visiteId={visite.Id}&prefillDate={date:yyyy-MM-dd}");
```

---

✅ **But du code**  
`[QueryProperty]` mappe automatiquement les paramètres de l'URL de navigation Shell vers les propriétés du ViewModel. Pas besoin d'implémenter `IQueryAttributable` ou d'analyser le dictionnaire manuellement.

⚙️ **Comment ça fonctionne en interne**  
La navigation Shell parse l'URL et affecte les paramètres aux propriétés annotées. Le setter généré par `[ObservableProperty]` est appelé, ce qui déclenche aussi `OnXxxChanged` si implémenté.

🔥 **Pourquoi c'est utile dans l'application**  
La navigation entre pages transmet des données complexes (identifiants, dates, modes). `[QueryProperty]` découple complètement la navigation de la logique ViewModel.

⚠️ **Bonne pratique**  
- `[QueryProperty]` passe les paramètres sous forme de `string` par défaut — utiliser `partial void OnXxxChanged` pour convertir si nécessaire (ex: `DateTime.TryParse`).
- Les paramètres de navigation ne doivent contenir que des identifiants — ne jamais passer des objets complexes sérialisés dans l'URL.

---

## 9. `WeakReferenceMessenger` — Communication entre ViewModels

### Concept

`WeakReferenceMessenger` permet à deux ViewModels de communiquer **sans se connaître directement**, en passant par un bus de messages. Cela respecte le principe de découplage MVVM.

### Exemple — `RapportViewModel` → `VisitDetailViewModel`

**Fichier envoyeur** : [ViewModels/Rapports/RapportViewModel.cs](../ViewModels/Rapports/RapportViewModel.cs)

```csharp
// Après soumission réussie du rapport
WeakReferenceMessenger.Default.Send(new VisiteCompletedMessage(LinkedVisiteId));
```

**Fichier récepteur** : [ViewModels/Visites/VisitDetailViewModel.cs](../ViewModels/Visites/VisitDetailViewModel.cs)

```csharp
public VisitDetailViewModel(...)
{
    WeakReferenceMessenger.Default.Register<VisiteCompletedMessage>(this, (_, m) =>
    {
        if (m.VisiteId == VisiteId)
            HasRapport = true;
        // Grâce à [NotifyPropertyChangedFor] :
        // ShowSubmitRapport → false
        // ShowEditRapport   → true
    });
}
```

**Message défini dans le projet :**

```csharp
// Messages/VisiteCompletedMessage.cs
public record VisiteCompletedMessage(int VisiteId);
```

---

✅ **But du code**  
Quand l'utilisateur soumet un rapport de visite (`RapportViewModel`), la page de détail de la visite (`VisitDetailViewModel`) doit se mettre à jour pour refléter que le rapport existe. Le messenger permet cette synchronisation sans que les deux ViewModels se référencent mutuellement.

⚙️ **Comment ça fonctionne en interne**  
- `WeakReferenceMessenger` utilise des **références faibles** : si `VisitDetailViewModel` est détruit (page fermée), l'abonnement est automatiquement nettoyé — pas de fuite mémoire.
- Un message est un simple objet (`record`) identifié par son type.
- N'importe quel ViewModel peut s'abonner à ce message.

🔥 **Pourquoi c'est utile dans l'application**  
Le workflow visite → rapport est un flux à double sens : après soumission du rapport, l'utilisateur revient à la page de visite qui doit afficher "Modifier le rapport" au lieu de "Soumettre un rapport". Sans messenger, il faudrait partager une référence directe entre les deux ViewModels, ce qui créerait un couplage fort.

⚠️ **Bonne pratique**  
- Toujours se désabonner dans `Dispose()` si le ViewModel implémente `IDisposable`, ou utiliser `ObservableRecipient` qui gère le cycle de vie automatiquement.
- Garder les messages simples (`record` avec juste les données nécessaires).
- Ne pas abuser du messenger — réserver son usage aux communications inter-ViewModels véritablement nécessaires.

---

## 10. Bilan et bénéfices dans le projet

### Tableau récapitulatif des fonctionnalités utilisées

| Fonctionnalité CommunityToolkit.MVVM | Nombre d'usages | ViewModels principaux |
|--------------------------------------|----------------|-----------------------|
| `[ObservableProperty]`               | 120+           | Tous les 25 ViewModels |
| `[RelayCommand]`                     | 90+            | Tous les 25 ViewModels |
| `[NotifyPropertyChangedFor]`         | 12+ ViewModels | Dashboard, VisitDetail, ChangePassword, ProductList... |
| `[NotifyDataErrorInfo]`              | 3 propriétés   | CreateOrder, Rapport, RapportViewModel |
| `ObservableValidator` (base)         | 1 (BaseViewModel) | Transversal à tout le projet |
| Partial Methods `OnXxxChanged`       | 15+ méthodes   | MesClients, VisitDetail, CreateOrder, Planning... |
| `[QueryProperty]`                    | 8 propriétés   | VisitDetail, Rapport, CreateOrder, DocumentDetail... |
| `WeakReferenceMessenger`             | 2 ViewModels   | RapportViewModel → VisitDetailViewModel |

---

### Comparaison : Avec vs Sans CommunityToolkit.MVVM

#### Sans le Toolkit (MVVM traditionnel)

```csharp
// LoginViewModel — Version SANS Toolkit
public class LoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(nameof(Email)); }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(nameof(Password)); }
    }

    private bool _isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        set { _isPasswordHidden = value; OnPropertyChanged(nameof(IsPasswordHidden)); }
    }

    private ICommand? _loginCommand;
    public ICommand LoginCommand =>
        _loginCommand ??= new Command(async () => await LoginAsync());

    private ICommand? _toggleCommand;
    public ICommand TogglePasswordVisibilityCommand =>
        _toggleCommand ??= new Command(() => IsPasswordHidden = !IsPasswordHidden);

    private void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ... LoginAsync, etc.
}
```

**Résultat : ~35 lignes** pour 3 propriétés + 2 commandes.

#### Avec le Toolkit (version du projet)

```csharp
// LoginViewModel — Version AVEC Toolkit (code réel du projet)
public partial class LoginViewModel : BaseViewModel
{
    [ObservableProperty] private string _email           = string.Empty;
    [ObservableProperty] private string _password        = string.Empty;
    [ObservableProperty] private bool   _isPasswordHidden = true;

    [RelayCommand]
    private Task LoginAsync() => ExecuteAsync(async () => { /* logique */ });

    [RelayCommand]
    private void TogglePasswordVisibility() => IsPasswordHidden = !IsPasswordHidden;
}
```

**Résultat : ~10 lignes** — une réduction de **70% du code boilerplate**.

---

### Bénéfices concrets pour le projet Cynapharm Mobile

**1. Productivité de développement**  
Avec 25 ViewModels et 120+ propriétés observables, le Toolkit a économisé l'écriture d'environ **1 500 lignes de code boilerplate**. Chaque nouvelle fonctionnalité se développe plus rapidement.

**2. Lisibilité et maintenabilité**  
Les ViewModels sont concis et expressifs. Un développeur qui rejoint le projet comprend immédiatement `[ObservableProperty]` et `[RelayCommand]` — il n'a pas besoin de déchiffrer des setters complexes.

**3. Fiabilité**  
Le code généré par le Toolkit est testé et maintenu par Microsoft. Les erreurs classiques (oublier `OnPropertyChanged`, fuite mémoire avec les commands) sont évitées automatiquement.

**4. Validation intégrée**  
`[NotifyDataErrorInfo]` avec `[Range]`, `[Required]`, `[MinLength]` offre une validation déclarative cohérente sur tous les formulaires (commandes, rapports, mots de passe) sans code de validation manuel.

**5. Architecture scalable**  
L'héritage `BaseViewModel → ObservableValidator` avec les helpers `ExecuteAsync`, `CheckConnectivityAsync`, et le pattern de cache hors ligne est réutilisé dans tous les ViewModels. Ajouter un nouveau ViewModel signifie hériter de ces capacités gratuitement.

**6. Communication découplée**  
`WeakReferenceMessenger` permet la synchronisation entre `RapportViewModel` et `VisitDetailViewModel` sans créer de dépendance directe, ce qui facilite les tests unitaires et l'évolution indépendante de chaque module.

---

### Conclusion

CommunityToolkit.MVVM est le **cœur architectural** de l'application Cynapharm Mobile. Son adoption systématique dans les 25 ViewModels du projet a permis de :

- ✅ Réduire drastiquement le code boilerplate
- ✅ Garantir la cohérence des notifications de propriétés
- ✅ Intégrer la validation de formulaires de manière déclarative
- ✅ Faciliter la communication entre ViewModels
- ✅ Maintenir une architecture MVVM propre et scalable

Pour un projet de fin d'études (PFE) qui implique une application mobile professionnelle avec authentification par rôles, gestion de commandes, rapports de visite géolocalisés et mode hors ligne, CommunityToolkit.MVVM représente le choix technique le plus adapté à l'écosystème .NET MAUI en 2025-2026.
