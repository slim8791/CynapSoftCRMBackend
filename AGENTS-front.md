# Cynapharm Mobile — Full UI Redesign Agent

## Your Mission

You are a UI redesign, navigation, and role-management agent for **Cynapharm Mobile**, a .NET MAUI CRM application for pharmaceutical sales teams.

**Your job has three parts:**
1. **Read and analyze the entire project** — frontend (XAML, ViewModels, AppShell, MauiProgram) AND backend logic (Services, ApiRoutes, models) — to fully understand what exists and how data flows before touching anything.
2. **Apply the redesign, navigation improvements, and role corrections** described in this document.
3. **You are allowed and encouraged to analyze backend code** (Services, ApiService, AuthService, LocalDatabaseService, Models, ApiRoutes) to understand the data contracts, but you must NEVER modify any backend file.

### Hard constraints — read before anything else
- **NEVER modify** backend files: `Services/`, `Models/`, `ApiRoutes.cs`, `LocalDatabaseService.cs`, `SyncService.cs`, `AuthService.cs`, `ApiService.cs`, or any handler/logger file.
- **NEVER break backend contracts** — all API endpoints, JSON field names, HTTP methods, and data models stay exactly as they are.
- **NEVER rename** existing bindings, commands, or `x:Name` attributes in XAML.
- **NEVER remove** the `SUPERVISEUR` role from the backend model or `UserInfo` — only hide its UI surfaces.
- **You MAY read** any `.cs` file to understand the logic. Reading is always allowed. Writing is restricted.
- **You MAY modify** only: XAML layout files, `Colors.xaml`, `Styles.xaml`, `AppShell.xaml`, `AppShell.xaml.cs` (navigation/visibility helpers only), and `App.xaml.cs` (login redirect logic only).

### Why you need to read the backend
Before redesigning any page, read the corresponding Service and ViewModel to understand:
- Which API fields are bound to which UI elements
- Which commands are called on which user actions
- Which properties are already computed (e.g. `CanDistribute`, `HasPromo`, `IsOffline`)
- Which navigation parameters are passed between pages
This prevents breaking any existing data flow when you restructure the XAML layout.

---

## Step 0 — Read and analyze the entire project

Before writing a single line, run these reads in order. Do not skip any step.

### Frontend reads
```
1. Read AppShell.xaml            → current navigation structure and flyout items
2. Read MauiProgram.cs           → DI registrations, registered pages and ViewModels
3. Read App.xaml.cs              → session/redirect logic, connectivity listeners
4. Read every Views/**/*.xaml    → inventory all pages, note all x:Name and bindings
5. Read every ViewModels/**/*.cs → understand all commands, properties, navigation calls
6. Read Resources/Styles/Colors.xaml
7. Read Resources/Styles/Styles.xaml
8. Read Converters/              → understand StatusColorConverter, InvertedBool, etc.
```

### Backend reads (read-only — NEVER modify)
```
9.  Read AuthService.cs          → role constants, JWT storage keys, session events
10. Read ApiRoutes.cs            → all endpoint paths grouped by domain
11. Read ApiService.cs           → ApiResponse<T> structure, error handling, headers
12. Read Models/**/*.cs          → all data models and their JSON field mappings
13. Read Services/ProductService.cs, OrderService.cs, VisiteService.cs,
         PlanningService.cs, KpiService.cs, InventoryService.cs,
         DocumentService.cs      → which endpoints each service calls
14. Read LocalDatabaseService.cs → SQLite tables, offline cache structure
15. Read SyncService.cs          → background sync behavior
16. Read AppSettings.cs          → API base URL
```

After completing all reads, build a mental map of:
- Which ViewModel feeds which page
- Which backend model fields appear in which XAML bindings
- Which commands call which service methods
- Which pages are role-restricted in the current code

Only then proceed to Step 1.

---

## Step 1 — Role Model Correction

### Backend roles (DO NOT CHANGE — read from AuthService.cs)
```
DELEGUE | SUPERVISEUR | PHARMACIEN | GROSSISTE | CLIENT | ADMIN | MEDECIN
```

### 3 UI scenarios to implement

| Scenario | Backend roles included | Landing page |
|---|---|---|
| **Délégué** | `DELEGUE`, `ADMIN`, `SUPERVISEUR` (fallback) | `//dashboard` |
| **Client** | `PHARMACIEN`, `GROSSISTE`, `CLIENT` | `//orders` |
| **Médecin** | `MEDECIN` | `//products` |

**Important — Client inheritance:**
`CLIENT` is the parent type. `PHARMACIEN` and `GROSSISTE` are subtypes. All three get identical screens. The only visual difference: `GROSSISTE` shows a volume KPI summary card on the order list header. Detect this with:
```csharp
bool IsGrossiste => _authService.UserRole is "GROSSISTE";
```

**Important — SUPERVISEUR:**
- Do NOT delete from backend model or `UserInfo`.
- Remove all SUPERVISEUR-specific UI (supervisor dashboard view, regions page, KPI supervisor cards).
- Redirect SUPERVISEUR to DELEGUE flow as fallback.
- Add comment: `// SUPERVISEUR role is deprecated in UI. Treated as DELEGUE for navigation.`

### Update role-to-redirect in App.xaml.cs (login success handler only)
```csharp
switch (role)
{
    case "DELEGUE":
    case "ADMIN":
    case "SUPERVISEUR":
        await Shell.Current.GoToAsync("//dashboard");
        break;
    case "PHARMACIEN":
    case "GROSSISTE":
    case "CLIENT":
        await Shell.Current.GoToAsync("//orders");
        break;
    case "MEDECIN":
        await Shell.Current.GoToAsync("//products");
        break;
}
```

---

## Step 2 — Color System

Update `Resources/Styles/Colors.xaml`:

```xml
<Color x:Key="Primary">#1A6B3C</Color>
<Color x:Key="PrimaryDark">#145530</Color>
<Color x:Key="PrimaryLight">#EAF3DE</Color>
<Color x:Key="PrimaryText">#3B6D11</Color>

<Color x:Key="Secondary">#F5A623</Color>
<Color x:Key="SecondaryLight">#FAEEDA</Color>
<Color x:Key="SecondaryText">#854F0B</Color>

<Color x:Key="Accent">#00B4D8</Color>
<Color x:Key="AccentLight">#E1F5FB</Color>

<Color x:Key="Danger">#E24B4A</Color>
<Color x:Key="DangerLight">#FCEBEB</Color>
<Color x:Key="DangerText">#A32D2D</Color>

<Color x:Key="InfoBackground">#E6F1FB</Color>
<Color x:Key="InfoText">#0C447C</Color>

<Color x:Key="PageBackground">#EEF3F8</Color>
<Color x:Key="CardBackground">#FFFFFF</Color>
<Color x:Key="SurfaceBackground">#F5F5F5</Color>
<Color x:Key="BorderColor">#E0E0E0</Color>

<Color x:Key="TextPrimary">#1A1A1A</Color>
<Color x:Key="TextSecondary">#6B6B6B</Color>
<Color x:Key="TextMuted">#9E9E9E</Color>
```

---

## Step 3 — Global Styles

Update `Resources/Styles/Styles.xaml`:

```xml
<!-- Page -->
<Style TargetType="ContentPage" ApplyToDerivedTypes="True">
    <Setter Property="BackgroundColor" Value="{StaticResource PageBackground}"/>
</Style>

<!-- Card -->
<Style x:Key="CardStyle" TargetType="Frame">
    <Setter Property="BackgroundColor" Value="{StaticResource CardBackground}"/>
    <Setter Property="BorderColor" Value="{StaticResource BorderColor}"/>
    <Setter Property="CornerRadius" Value="12"/>
    <Setter Property="Padding" Value="14"/>
    <Setter Property="HasShadow" Value="False"/>
    <Setter Property="Margin" Value="0,0,0,10"/>
</Style>

<!-- Primary button -->
<Style x:Key="PrimaryButtonStyle" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource Primary}"/>
    <Setter Property="TextColor" Value="White"/>
    <Setter Property="FontAttributes" Value="Bold"/>
    <Setter Property="FontSize" Value="15"/>
    <Setter Property="HeightRequest" Value="50"/>
    <Setter Property="CornerRadius" Value="10"/>
    <Setter Property="Margin" Value="0,8,0,0"/>
</Style>

<!-- Secondary button (outline) -->
<Style x:Key="SecondaryButtonStyle" TargetType="Button">
    <Setter Property="BackgroundColor" Value="Transparent"/>
    <Setter Property="TextColor" Value="{StaticResource Primary}"/>
    <Setter Property="BorderColor" Value="{StaticResource Primary}"/>
    <Setter Property="BorderWidth" Value="1"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="HeightRequest" Value="44"/>
    <Setter Property="CornerRadius" Value="10"/>
</Style>

<!-- Danger button -->
<Style x:Key="DangerButtonStyle" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource DangerLight}"/>
    <Setter Property="TextColor" Value="{StaticResource DangerText}"/>
    <Setter Property="BorderColor" Value="#F7C1C1"/>
    <Setter Property="BorderWidth" Value="1"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontAttributes" Value="Bold"/>
    <Setter Property="HeightRequest" Value="48"/>
    <Setter Property="CornerRadius" Value="10"/>
</Style>

<!-- Section title -->
<Style x:Key="SectionTitleStyle" TargetType="Label">
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontAttributes" Value="Bold"/>
    <Setter Property="TextColor" Value="{StaticResource TextPrimary}"/>
    <Setter Property="Margin" Value="0,0,0,8"/>
</Style>

<!-- Muted label -->
<Style x:Key="MutedLabelStyle" TargetType="Label">
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="TextColor" Value="{StaticResource TextSecondary}"/>
</Style>

<!-- Entry -->
<Style TargetType="Entry" ApplyToDerivedTypes="True">
    <Setter Property="BackgroundColor" Value="{StaticResource SurfaceBackground}"/>
    <Setter Property="TextColor" Value="{StaticResource TextPrimary}"/>
    <Setter Property="PlaceholderColor" Value="{StaticResource TextMuted}"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="HeightRequest" Value="48"/>
    <Setter Property="Margin" Value="0,0,0,12"/>
</Style>

<!-- Progress bar -->
<Style TargetType="ProgressBar">
    <Setter Property="ProgressColor" Value="{StaticResource Primary}"/>
    <Setter Property="BackgroundColor" Value="{StaticResource SurfaceBackground}"/>
    <Setter Property="HeightRequest" Value="6"/>
</Style>
```

### Status badge color reference (use in StatusColorConverter or inline)
```
PLANIFIEE  → Background #FAEEDA, Text #854F0B
REALISEE   → Background #EAF3DE, Text #3B6D11
ANNULEE    → Background #FCEBEB, Text #A32D2D
EN_ATTENTE → Background #FAEEDA, Text #854F0B
CONFIRMEE  → Background #EAF3DE, Text #3B6D11
LIVREE     → Background #EAF3DE, Text #3B6D11
```

### Global layout rules for ALL pages
- Every page header: `BackgroundColor="{StaticResource Primary}"`, white text
- All `Frame`: `HasShadow="False"`, `BorderColor="{StaticResource BorderColor}"`
- All buttons: minimum `HeightRequest="44"`
- Font sizes: 18pt page title, 14pt section title, 14pt body, 12pt secondary, 11pt hint (never below 11pt)
- CornerRadius: cards 12, buttons 10, chips/badges 20, small elements 8
- Page horizontal padding 16px, card padding 14px, gap between cards 10px
- Use `CollectionView` instead of `ListView` everywhere

---

## Step 4 — AppShell.xaml

Add role helpers to `AppShell.xaml.cs`:
```csharp
public bool IsDelegue => Role is "DELEGUE" or "ADMIN" or "SUPERVISEUR";
public bool IsClient  => Role is "PHARMACIEN" or "GROSSISTE" or "CLIENT";
public bool IsMedecin => Role is "MEDECIN";
```

### Flyout header structure
```xml
<Shell.FlyoutHeader>
    <Grid BackgroundColor="{StaticResource Primary}" Padding="16,48,16,20">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="44"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Frame Grid.Column="0" WidthRequest="44" HeightRequest="44"
               CornerRadius="22" BackgroundColor="#33FFFFFF"
               BorderColor="Transparent" Padding="0" HasShadow="False">
            <Label Text="{Binding UserInitials}" FontSize="16" FontAttributes="Bold"
                   TextColor="White" HorizontalOptions="Center" VerticalOptions="Center"/>
        </Frame>
        <StackLayout Grid.Column="1" Margin="10,0,0,0" VerticalOptions="Center">
            <Label Text="{Binding UserName}" FontSize="15" FontAttributes="Bold" TextColor="White"/>
            <Label Text="{Binding UserRole}" FontSize="12" TextColor="#99FFFFFF"/>
        </StackLayout>
    </Grid>
</Shell.FlyoutHeader>
```

### Flyout items by role (use IsVisible bindings)

**DELEGUE / ADMIN / SUPERVISEUR (IsDelegue = true):**
- Dashboard
- Mes visites
- Planning
- Catalogue
- Commandes
- Mon stock
- Profil

**CLIENT / PHARMACIEN / GROSSISTE (IsClient = true):**
- Mes commandes (landing)
- Catalogue
- Documents
- Profil

**MEDECIN (IsMedecin = true):**
- Médicaments / Catalogue (landing)
- Profil

### Active item style
- Active item background: `BackgroundColor="#26FFFFFF"`
- Orange accent bar right side: `BoxView` 4×24, `BackgroundColor="{StaticResource Secondary}"`

### Flyout footer
```xml
<Shell.FlyoutFooter>
    <StackLayout Padding="16,12" BackgroundColor="{StaticResource Primary}">
        <Label Text="Cynapharm CRM v1.0.0"
               FontSize="11" TextColor="#66FFFFFF" HorizontalOptions="Center"/>
    </StackLayout>
</Shell.FlyoutFooter>
```

---

## Step 4b — Professional Navigation System

This is a critical step. After redesigning AppShell, implement a complete professional navigation experience. The goal is a fast, fluid, context-aware navigation that feels native on Android.

### Navigation architecture choice

Use a **hybrid approach**: Bottom Tab Bar for primary destinations (always visible, 1-tap access) + Flyout for secondary actions (profile, logout, settings). This is the industry standard for field CRM apps on Android.

#### Bottom tab bar — per role

**DELEGUE / ADMIN:**
```
Tab 1: Dashboard    (ti-home icon)
Tab 2: Visites      (ti-clipboard-list icon)   ← most used on terrain
Tab 3: Planning     (ti-calendar icon)
Tab 4: Catalogue    (ti-pill icon)
Tab 5: Plus…        (ti-menu-2 icon)  → opens flyout for: Stock, Commandes, Objectifs, Profil
```

**CLIENT (PHARMACIEN / GROSSISTE):**
```
Tab 1: Commandes    (ti-shopping-cart icon)    ← landing
Tab 2: Catalogue    (ti-pill icon)
Tab 3: Documents    (ti-file icon)
Tab 4: Profil       (ti-user icon)
```

**MEDECIN:**
```
Tab 1: Médicaments  (ti-pill icon)             ← only tab
Tab 2: Profil       (ti-user icon)
```

#### Implementation in MAUI Shell
Use `TabBar` with `Tab` items for primary navigation. Keep `FlyoutItem` for secondary pages accessible from the "Plus" tab or hamburger.

```xml
<!-- Example structure for DELEGUE -->
<TabBar x:Name="DelegueTabBar" IsVisible="{Binding IsDelegue}">
    <Tab Title="Accueil" Icon="tab_home.png" Route="dashboard">
        <ShellContent ContentTemplate="{DataTemplate views:DashboardPage}"/>
    </Tab>
    <Tab Title="Visites" Icon="tab_visites.png" Route="visits">
        <ShellContent ContentTemplate="{DataTemplate views:VisitListPage}"/>
    </Tab>
    <Tab Title="Planning" Icon="tab_planning.png" Route="planning">
        <ShellContent ContentTemplate="{DataTemplate views:PlanningPage}"/>
    </Tab>
    <Tab Title="Catalogue" Icon="tab_catalogue.png" Route="products">
        <ShellContent ContentTemplate="{DataTemplate views:ProductListPage}"/>
    </Tab>
</TabBar>
```

For tab icons, use simple monochrome SVG or PNG assets at 24×24dp. Active tab color: `Primary (#1A6B3C)`. Inactive: `TextSecondary (#6B6B6B)`. Tab bar background: white, top border `0.5px BorderColor`.

### Navigation performance rules

**1. Lazy loading — never eagerly instantiate pages**
```xml
<!-- Use ContentTemplate (lazy) not Content (eager) -->
<ShellContent ContentTemplate="{DataTemplate views:DashboardPage}"/>  ✓
<ShellContent>
    <views:DashboardPage/>  <!-- WRONG — instantiated at startup -->
</ShellContent>
```

**2. Page caching — enable for heavy pages**
Add to pages that are frequently revisited:
```xml
<ContentPage ... shell:Shell.PresentationMode="Animated">
```
And in the ViewModel constructor, check if data is already loaded before re-fetching:
```csharp
// In LoadDataAsync — already exists in BaseViewModel pattern
if (Items.Any() && !IsRefreshing) return; // skip reload if cached
```

**3. Navigation transitions — smooth and fast**
Set on all `ContentPage`:
```xml
shell:Shell.PresentationMode="Animated"
```
For modal pages (rapport form, réclamation form):
```xml
shell:Shell.PresentationMode="ModalAnimated"
```

**4. Back navigation — consistent behavior**
- Always show back arrow in header for detail/form pages
- Use `Shell.BackButtonBehavior` for custom back logic where needed
- Never use `Application.Current.MainPage` for navigation — always `Shell.Current.GoToAsync()`

**5. CollectionView performance — mandatory for all lists**
```xml
<CollectionView ItemsSource="{Binding Items}"
                RemainingItemsThreshold="3"
                RemainingItemsThresholdReachedCommand="{Binding LoadMoreCommand}">
    <CollectionView.ItemsLayout>
        <LinearItemsLayout Orientation="Vertical" ItemSpacing="10"/>
    </CollectionView.ItemsLayout>
    <!-- Use DataTemplate with RecycleElement -->
</CollectionView>
```

**6. Empty states — every list page must have one**
```xml
<CollectionView.EmptyView>
    <StackLayout VerticalOptions="Center" HorizontalOptions="Center" Padding="40">
        <Label Text="&#xe9cb;" FontFamily="Tabler" FontSize="48"
               TextColor="{StaticResource TextMuted}" HorizontalOptions="Center"/>
        <Label Text="Aucun élément trouvé" FontSize="15"
               TextColor="{StaticResource TextMuted}"
               HorizontalOptions="Center" Margin="0,12,0,0"/>
    </StackLayout>
</CollectionView.EmptyView>
```

**7. Loading states — IsBusy spinner on every page**
Every page must show an `ActivityIndicator` when `IsBusy = true`:
```xml
<ActivityIndicator IsRunning="{Binding IsBusy}"
                   IsVisible="{Binding IsBusy}"
                   Color="{StaticResource Primary}"
                   VerticalOptions="Center" HorizontalOptions="Center"/>
```
Wrap list content in a `Grid` so spinner overlays the list cleanly.

**8. Pull-to-refresh — on all list pages**
```xml
<RefreshView IsRefreshing="{Binding IsRefreshing}"
             Command="{Binding RefreshCommand}"
             RefreshColor="{StaticResource Primary}">
    <CollectionView .../>
</RefreshView>
```

**9. Keyboard and safe area handling**
```xml
<!-- On all pages with Entry fields -->
<ContentPage ...>
    <ScrollView>
        <StackLayout Padding="16,0,16,{OnPlatform Android=80, iOS=100}">
            <!-- content -->
        </StackLayout>
    </ScrollView>
</ContentPage>
```

**10. Navigation parameter passing — use Shell query parameters**
```csharp
// Navigate with params (read from existing ViewModel navigation calls)
await Shell.Current.GoToAsync($"visits/rapport?visiteId={visite.Id}");
// Receive (keep existing [QueryProperty] attributes unchanged)
```

### Flyout professional design (secondary menu)

The flyout (opened via hamburger or "Plus" tab) handles secondary navigation. Design rules:

- **Width:** 280dp (standard Android drawer width)
- **Header:** Primary green background, avatar + name + role badge (same as Step 4)
- **Menu items:** 52dp height, 16dp horizontal padding, 12dp icon-text gap
- **Active item:** `BackgroundColor="#1A1A6B3C"` (Primary 10% alpha) + left accent bar 3×24 `BackgroundColor=Secondary`
- **Sections:** divider lines + section labels (10pt, muted, uppercase, 8dp padding)
- **Footer:** version number + logout button (danger style)

Menu item structure:
```xml
<Grid HeightRequest="52" Padding="16,0">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="36"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    <!-- Icon box -->
    <Frame Grid.Column="0" WidthRequest="36" HeightRequest="36"
           CornerRadius="8" BackgroundColor="#1A1A6B3C"
           HasShadow="False" Padding="0" BorderColor="Transparent">
        <Label Text="{StaticResource IconCode}" FontFamily="Tabler"
               FontSize="18" TextColor="{StaticResource Primary}"
               HorizontalOptions="Center" VerticalOptions="Center"/>
    </Frame>
    <!-- Label -->
    <Label Grid.Column="1" Text="Menu Item" FontSize="14"
           TextColor="{StaticResource TextPrimary}"
           VerticalOptions="Center" Margin="12,0,0,0"/>
    <!-- Badge (optional) -->
    <Frame Grid.Column="2" IsVisible="{Binding HasBadge}"
           BackgroundColor="{StaticResource Secondary}"
           CornerRadius="10" Padding="6,2" HasShadow="False" BorderColor="Transparent">
        <Label Text="{Binding BadgeCount}" FontSize="11"
               TextColor="{StaticResource SecondaryText}" FontAttributes="Bold"/>
    </Frame>
</Grid>
```

### Notification badge on tab bar

For DELEGUE: show a badge on the Visites tab when there are pending offline rapports:
```xml
<Tab Title="Visites" Icon="tab_visites.png">
    <!-- Badge overlay — bind to PendingRapportsCount from SyncService -->
    <Tab.IconImageSource>
        <!-- Use FontImageSource or a custom tab renderer -->
    </Tab.IconImageSource>
</Tab>
```
Bind `PendingRapportsCount` from `SyncService.GetPendingRapportsAsync()` count. Show red dot badge when count > 0.

---

Read each existing XAML before rewriting. Preserve all binding paths and x:Name attributes.

---

### 5.1 LoginPage.xaml (ALL roles)

**Top zone** (Primary background, `Padding="24,60,24,48"`):
- Icon box (60×60, `CornerRadius=16`, `BackgroundColor="#26FFFFFF"`) with pill icon
- App name: `"Cynapharm CRM"`, 22pt, bold, white
- Subtitle: `"CynaSoft"`, 13pt, white 60% opacity

**Bottom sheet** (white, `CornerRadius` top 16px, overlapping top zone by -24px):
- `"Connexion"` title, 18pt bold
- Email `Entry`
- Password `Entry` (`IsPassword="True"`) + eye icon visibility toggle
- Primary button `"Se connecter"` → existing `LoginCommand`
- Forgot password `Label` (Primary color) → `ForgotPasswordPage`

---

### 5.2 DashboardPage.xaml (DELEGUE / ADMIN only)

Remove ALL SUPERVISEUR-specific sections. Keep DELEGUE view only.

**Header** (Primary, `Padding="16,48,16,64"`):
- Row: avatar (40×40 circle, initials) + welcome column + bell icon
- Date subtitle (white 60%)

**KPI cards** (overlap header by `-28` margin via `Grid`):
- 2-col grid of `Frame` (`CardStyle`)
- Card 1: visits today count
- Card 2: visits done count

**Quick access** (2-col button grid):
- `"Mes visites"` → filled Primary
- `"Planning"` → surface with border

**Objectives** (`CollectionView`):
- Label + value + `ProgressBar`
- Progress color: ≥80% → Primary, 50–79% → Secondary, <50% → Danger

---

### 5.3 VisitListPage.xaml (DELEGUE / ADMIN only)

**Header** (Primary): title + search bar (white 15% alpha background).

**Filter chips** (horizontal `ScrollView`):
- Active: `BackgroundColor=Primary`, white, `CornerRadius=20`
- Inactive: `BackgroundColor=SurfaceBackground`, muted, border

**Visit list** (`CollectionView`):
- Each `Frame` (`CardStyle`): client name + status badge / date + time / action buttons row
- Action buttons: `"Créer rapport"` (Primary) + `"Détail"` (surface)

**FAB:** `"+ Nouvelle visite"`, dashed border, Primary color, bottom.

---

### 5.4 VisitDetailPage.xaml (DELEGUE / ADMIN only)

**Header** (Primary): back arrow + `"Détail visite"`.
Form: client name, date picker, notes editor, save button (`PrimaryButtonStyle`).

---

### 5.5 RapportPage.xaml (DELEGUE / ADMIN only)

**Header** (Primary): back arrow + `"Rapport de visite"` + client/date subtitle.

Fields:
- Content `Editor` (`SurfaceBackground`, min height 100) + `"Minimum 20 caractères"` hint
- Result selector: 3 `Frame` buttons (Positif / Négatif / En attente) — selected = Primary bg
- Products `CollectionView` checkboxes — checked = Primary checkbox + Primary border
- GPS status `Frame` (`BackgroundColor=PrimaryLight`) with 3 bound states
- Submit button (`PrimaryButtonStyle`)

---

### 5.6 PlanningPage.xaml (DELEGUE / ADMIN only)

**Header** (Primary): left arrow + `"Planning — Semaine XX"` + right arrow + week range.

**Day picker** (7-cell horizontal `CollectionView`):
- Selected cell: `BackgroundColor=Primary`, white, `CornerRadius=8`

**Planning list** (`CollectionView`):
- Each `Frame`: left `BoxView` accent (3px, status-colored) + client + time + badge
- Colors: PLANIFIEE → Secondary, REALISEE → Primary, ANNULEE → Danger

**FAB:** `"+ Ajouter une visite"`, full width, Primary.

---

### 5.7 ProductListPage.xaml (ALL roles — different views)

**Header** (Primary): title + search bar.

**Offline banner** (`IsVisible` bound to `IsOffline`):
- `BackgroundColor=SecondaryLight`, `TextColor=SecondaryText`

**Category chips** (horizontal `ScrollView`).

**Product cards** (`CollectionView`):

For DELEGUE / ADMIN / CLIENT — show prices:
```
[icon box]  Name (bold 14pt)
            Category (muted 12pt)
            Price: struck-through + discounted (Primary) + promo badge
            → tap navigates to full ProductDetailPage
```

For MEDECIN — no prices, no order actions:
```
[icon box]  Name (bold 14pt)
            Category (muted 12pt)
            "Disponible" green badge only
            → tap navigates to read-only ProductDetailPage
```

Use `IsVisible` bound to a ViewModel bool property `CanSeePrices`:
```csharp
public bool CanSeePrices => _authService.UserRole is not "MEDECIN";
```

---

### 5.8 ProductDetailPage.xaml

For DELEGUE / ADMIN / CLIENT:
- Full fiche: description, price, lots, active promotions, marketing materials.

For MEDECIN:
- Description only + marketing materials (PDF downloads).
- **Hide** using `IsVisible`: price row, lots section, promotions section, any order/cart button.
- Show info banner (`BackgroundColor=InfoBackground`):
  `"Pour toute demande d'échantillons, contactez votre délégué Cynapharm."`

---

### 5.9 OrderListPage.xaml (DELEGUE / ADMIN / CLIENT only)

**Header** (Primary):
- Avatar row + name + `"+ Nouvelle"` button
- For GROSSISTE only (`IsVisible="{Binding IsGrossiste}"`): 2-col KPI cards (orders count + volume)
- Filter chips row: Tous / En attente / Confirmée / Livrée / Annulée

**Order list** (`CollectionView`, 20 per page):
- Each `Frame` (`CardStyle`): order number + status badge / date + count / amount + `"Voir détail"`

**"Charger plus"** surface button at bottom.

---

### 5.10 CreateOrderPage.xaml (DELEGUE / ADMIN / CLIENT only)

**Wizard progress bar** in header (3 steps, active = white, inactive = white 30%).

**Step 1 — Cart:**
- Search bar
- Cart `CollectionView`: name + promo badge + price row + quantity stepper + line total
- Summary `Frame`: subtotal + savings (green, hidden if 0) + divider + total
- `"Suivant — Notes de livraison"` (`PrimaryButtonStyle`)

**Step 2 — Delivery:**
- Address field (pre-filled), instructions `Editor`, date picker
- `"Suivant — Confirmer"` (`PrimaryButtonStyle`)

**Step 3 — Confirmation:**
- Success circle (64×64, `BackgroundColor=PrimaryLight`, checkmark)
- `"Commande confirmée !"` + order number
- Summary card: articles + savings (if any) + total
- `"Voir mes commandes"` + `"Nouvelle commande"` buttons

---

### 5.11 OrderDetailPage.xaml (DELEGUE / ADMIN / CLIENT only)

**Header** (Primary): back arrow + order number + status badge.
**Info cards** (2-col): date + total HT.
**Articles list**: name + qty × unit price + promo % (green) + line total.
**Totals**: subtotal + savings + total HT.
**"Refaire cette commande"** button (surface style, copy icon).
**Réclamation access** (LIVREE orders only): `Frame` `BackgroundColor=DangerLight` + alert icon + arrow.

---

### 5.12 DocumentListPage.xaml (CLIENT / ADMIN only)

**Header** (Primary): `"Mes documents"` + 3-segment control (Factures / Bons cmd. / Bons livr.).
Active segment: white pill. Inactive: transparent white 80%.

**Month filter**: calendar icon + month/year + chevron.

**Document list** (`CollectionView`):
- Each `Frame` (`CardStyle`): icon box (type-colored) + number + date / amount + status badge.

**Pagination** at bottom.

---

### 5.13 DocumentDetailPage.xaml (CLIENT / ADMIN only)

**Header** (Primary): back arrow + document number + status badge.
Keep existing binding structure, apply visual styles only.

---

### 5.14 MyStockPage.xaml (DELEGUE / ADMIN only)

**Header** (Primary): `"Mon stock"`.
**Tab bar**: Échantillons / Promotionnels. Active: bottom border 2px Primary.

**Échantillons tab** (`CollectionView`):
- Each `Frame` (`CardStyle`): name + X/Y + expiry + `ProgressBar` + `"Distribuer"` button
- `ProgressBar`: Primary if >30% remaining, Secondary if ≤30%
- Button `IsEnabled` bound to `CanDistribute`

**Success snackbar** (`BackgroundColor=PrimaryLight`): auto-dismiss 3 seconds.

---

### 5.15 ObjectifPage.xaml (DELEGUE / ADMIN only)

Remove SUPERVISEUR content. DELEGUE view only.

**Header** (Primary): `"Mes objectifs"` + global achievement card (white 15% alpha).

**Objectives `CollectionView`**:
- Each `Frame`: type label + period badge / `ProgressBar` (colored by %) / values row
- Colors: ≥80% Primary, 50–79% Secondary, <50% Danger

---

### 5.16 ProfilePage.xaml (ALL roles)

**Header** (Primary, tall): avatar 72×72 + name + role badge pill.

**Fields** (bottom-bordered rows, icon box 36×36 `BackgroundColor=PrimaryLight`):

```
ALL roles:   email, phone
DELEGUE:     + region
CLIENT:      + establishment name + address
MEDECIN:     + cabinet name + wilaya + assigned delegate (read-only blue info box)
```

**Assigned delegate box** (MEDECIN only, `BackgroundColor=InfoBackground`):
- Delegate icon + `"Délégué assigné"` label + delegate name bound to ViewModel.

**Action buttons**:
- `"Modifier le profil"` → `SecondaryButtonStyle`
- `"Changer le mot de passe"` → `SecondaryButtonStyle` → `ForgotPasswordPage`
- `"Se déconnecter"` → `DangerButtonStyle` → existing `LogoutCommand`

---

### 5.17 ReclamationPage.xaml (CLIENT / ADMIN only)

**Header** (Primary): back arrow + `"Réclamation"` + order subtitle.

**Warning banner** (`BackgroundColor=SecondaryLight`): alert icon + 48h processing notice.

**Form**:
- Order reference (read-only, pre-filled)
- Problem type: radio-style `Frame` list (selected = Primary border + checkbox)
  - Options: `"Produit abîmé"` / `"Quantité incorrecte"` / `"Produit non conforme"` / `"Autre"`
- Description `Editor` (`SurfaceBackground`, min 80px)

**Submit**: `DangerButtonStyle`, `"Soumettre la réclamation"` → existing `CreateReclamationCommand`

---

## Step 6 — Pages to hide (not delete)

Do not add flyout items or navigation paths to these for the 3 active roles:
- Any SUPERVISEUR-only dashboard variant
- Regions page (if standalone)

---

## Step 7 — Execution Order

```
1.  Resources/Styles/Colors.xaml
2.  Resources/Styles/Styles.xaml
3.  App.xaml.cs                        (role redirect only)
4.  AppShell.xaml + AppShell.xaml.cs   (flyout + role helpers)
5.  Views/Auth/LoginPage.xaml
6.  Views/Dashboard/DashboardPage.xaml
7.  Views/Planning/PlanningPage.xaml
8.  Views/Visites/VisitListPage.xaml
9.  Views/Visites/VisitDetailPage.xaml
10. Views/Rapports/RapportPage.xaml
11. Views/Products/ProductListPage.xaml
12. Views/Products/ProductDetailPage.xaml
13. Views/Orders/OrderListPage.xaml
14. Views/Orders/CreateOrderPage.xaml
15. Views/Orders/OrderDetailPage.xaml
16. Views/Documents/DocumentListPage.xaml
17. Views/Documents/DocumentDetailPage.xaml
18. Views/Stock/MyStockPage.xaml
19. Views/Objectifs/ObjectifPage.xaml
20. Views/Profile/ProfilePage.xaml
21. Views/Orders/ReclamationPage.xaml
```

---

## Step 8 — Verification Checklist

After all changes, verify every scenario:

**Scenario 1 — Délégué:**
- [ ] Login with `DELEGUE` → lands on Dashboard
- [ ] Bottom tab bar shows: Dashboard, Visites, Planning, Catalogue, Plus
- [ ] Sees: visites, planning, catalogue (with prices), commandes, stock, objectifs
- [ ] Does NOT see: documents page, réclamation form

**Scenario 2 — Client:**
- [ ] Login with `PHARMACIEN` → lands on OrderList, no KPI header cards
- [ ] Login with `GROSSISTE` → lands on OrderList, KPI volume header cards visible
- [ ] Login with `CLIENT` → same as PHARMACIEN
- [ ] Bottom tab bar shows: Commandes, Catalogue, Documents, Profil
- [ ] Sees: commandes, catalogue (with prices), documents, profil
- [ ] Does NOT see: dashboard, planning, rapports, stock, objectifs

**Scenario 3 — Médecin:**
- [ ] Login with `MEDECIN` → lands on ProductList
- [ ] Bottom tab bar shows: Médicaments, Profil only
- [ ] Sees: catalogue (NO prices, NO order/cart buttons), profil with assigned delegate
- [ ] Does NOT see: commandes, documents, stock, dashboard, planning

**Fallbacks:**
- [ ] Login with `SUPERVISEUR` → redirected to Dashboard (DELEGUE flow), no crash
- [ ] Login with `ADMIN` → same as DELEGUE

**Navigation performance:**
- [ ] All `ShellContent` uses `ContentTemplate` (lazy loading) — no eager page creation
- [ ] All list pages have pull-to-refresh (`RefreshView`)
- [ ] All list pages have empty state UI
- [ ] All list pages have `ActivityIndicator` overlaid when `IsBusy = true`
- [ ] Back navigation works correctly on all detail/form pages
- [ ] No navigation uses `Application.Current.MainPage` — all use `Shell.Current.GoToAsync()`
- [ ] `CollectionView` used everywhere (no `ListView`)
- [ ] Flyout opens smoothly, items are 52dp height with icon boxes

**Technical:**
- [ ] All existing API calls still work — no endpoint or model changes
- [ ] All `x:Name` and binding paths unchanged
- [ ] No hardcoded colors — all use `StaticResource`
- [ ] Every page has green Primary header
- [ ] `HasShadow="False"` on all `Frame` elements
- [ ] All buttons minimum 44px height
- [ ] No backend file has been modified
