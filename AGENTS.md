# Cynapharm Mobile — UI Redesign Agent

## Mission

You are a UI redesign agent for **Cynapharm Mobile**, a .NET MAUI CRM application for pharmaceutical sales teams. Your task is to update the visual design of all XAML pages to match the new design system defined in this document. Do NOT change any business logic, ViewModels, Services, or Models — only touch XAML files and `Colors.xaml` / `Styles.xaml`.

---

## Brand & Color System

Apply these exact colors everywhere. Update `Resources/Styles/Colors.xaml` first.

```xml
<!-- Primary brand colors -->
<Color x:Key="Primary">#1A6B3C</Color>
<Color x:Key="PrimaryDark">#145530</Color>
<Color x:Key="PrimaryLight">#EAF3DE</Color>
<Color x:Key="PrimaryText">#3B6D11</Color>

<!-- Secondary / accent -->
<Color x:Key="Secondary">#F5A623</Color>
<Color x:Key="SecondaryLight">#FAEEDA</Color>
<Color x:Key="SecondaryText">#854F0B</Color>

<!-- Cyan accent -->
<Color x:Key="Accent">#00B4D8</Color>
<Color x:Key="AccentLight">#E1F5FB</Color>

<!-- Danger / error -->
<Color x:Key="Danger">#E24B4A</Color>
<Color x:Key="DangerLight">#FCEBEB</Color>
<Color x:Key="DangerText">#A32D2D</Color>

<!-- Neutral surfaces -->
<Color x:Key="PageBackground">#EEF3F8</Color>
<Color x:Key="CardBackground">#FFFFFF</Color>
<Color x:Key="SurfaceBackground">#F5F5F5</Color>
<Color x:Key="BorderColor">#E0E0E0</Color>

<!-- Text -->
<Color x:Key="TextPrimary">#1A1A1A</Color>
<Color x:Key="TextSecondary">#6B6B6B</Color>
<Color x:Key="TextMuted">#9E9E9E</Color>
```

---

## Global Styles (`Resources/Styles/Styles.xaml`)

Define these reusable styles. Every page must use them — never hardcode colors or sizes inline when a style exists.

### Page
```xml
<Style TargetType="ContentPage" ApplyToDerivedTypes="True">
    <Setter Property="BackgroundColor" Value="{StaticResource PageBackground}" />
</Style>
```

### Top Header Bar
Every page has a colored header. Use this pattern:
```xml
<Grid BackgroundColor="{StaticResource Primary}" Padding="16,48,16,16">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>
    <!-- Back button (col 0), Title (col 1), Action icon (col 2) -->
    <Label Grid.Column="1"
           Text="Page Title"
           FontSize="18"
           FontAttributes="Bold"
           TextColor="White"
           VerticalOptions="Center"
           HorizontalOptions="Center"/>
</Grid>
```

### Cards
```xml
<Style x:Key="CardStyle" TargetType="Frame">
    <Setter Property="BackgroundColor" Value="{StaticResource CardBackground}"/>
    <Setter Property="BorderColor" Value="{StaticResource BorderColor}"/>
    <Setter Property="CornerRadius" Value="12"/>
    <Setter Property="Padding" Value="14"/>
    <Setter Property="HasShadow" Value="False"/>
    <Setter Property="Margin" Value="0,0,0,10"/>
</Style>
```

### Primary Button
```xml
<Style x:Key="PrimaryButtonStyle" TargetType="Button">
    <Setter Property="BackgroundColor" Value="{StaticResource Primary}"/>
    <Setter Property="TextColor" Value="White"/>
    <Setter Property="FontAttributes" Value="Bold"/>
    <Setter Property="FontSize" Value="15"/>
    <Setter Property="HeightRequest" Value="50"/>
    <Setter Property="CornerRadius" Value="10"/>
    <Setter Property="Margin" Value="0,8,0,0"/>
</Style>
```

### Secondary Button (outline)
```xml
<Style x:Key="SecondaryButtonStyle" TargetType="Button">
    <Setter Property="BackgroundColor" Value="Transparent"/>
    <Setter Property="TextColor" Value="{StaticResource Primary}"/>
    <Setter Property="BorderColor" Value="{StaticResource Primary}"/>
    <Setter Property="BorderWidth" Value="1"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="HeightRequest" Value="44"/>
    <Setter Property="CornerRadius" Value="10"/>
</Style>
```

### Status Badge
Use `Frame` with `CornerRadius="12"` and `Padding="6,3"`. Colors:
- `PLANIFIEE` → Background `#FAEEDA`, TextColor `#854F0B`
- `REALISEE` → Background `#EAF3DE`, TextColor `#3B6D11`
- `ANNULEE` → Background `#FCEBEB`, TextColor `#A32D2D`
- `EN_ATTENTE` → Background `#FAEEDA`, TextColor `#854F0B`
- `CONFIRMEE` / `LIVREE` → Background `#EAF3DE`, TextColor `#3B6D11`

### Section Title
```xml
<Style x:Key="SectionTitleStyle" TargetType="Label">
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontAttributes" Value="Bold"/>
    <Setter Property="TextColor" Value="{StaticResource TextPrimary}"/>
    <Setter Property="Margin" Value="0,0,0,8"/>
</Style>
```

### Muted Label
```xml
<Style x:Key="MutedLabelStyle" TargetType="Label">
    <Setter Property="FontSize" Value="12"/>
    <Setter Property="TextColor" Value="{StaticResource TextSecondary}"/>
</Style>
```

### Entry (input field)
```xml
<Style TargetType="Entry" ApplyToDerivedTypes="True">
    <Setter Property="BackgroundColor" Value="{StaticResource SurfaceBackground}"/>
    <Setter Property="TextColor" Value="{StaticResource TextPrimary}"/>
    <Setter Property="PlaceholderColor" Value="{StaticResource TextMuted}"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="HeightRequest" Value="48"/>
    <Setter Property="Margin" Value="0,0,0,12"/>
</Style>
```

### Progress Bar
```xml
<Style TargetType="ProgressBar">
    <Setter Property="ProgressColor" Value="{StaticResource Primary}"/>
    <Setter Property="BackgroundColor" Value="{StaticResource SurfaceBackground}"/>
    <Setter Property="HeightRequest" Value="6"/>
</Style>
```

---

## Page-by-Page Redesign Instructions

---

### 1. `Views/Auth/LoginPage.xaml`

**Layout:** Two-zone vertical layout — colored top zone + white bottom sheet.

**Top zone** (`BackgroundColor=Primary`, `Padding="24,60,24,48"`):
- Centered logo icon (`ImageSource="pharmacy_icon.png"` or a `Label` with emoji fallback `💊`) in a rounded white-tinted box (60×60, `CornerRadius=16`, `BackgroundColor` with 20% white alpha)
- App name: `"Cynapharm CRM"`, `FontSize=22`, `TextColor=White`, `FontAttributes=Bold`
- Subtitle: `"CynaSoft — Espace commercial"`, `FontSize=13`, `TextColor` white at 60% opacity

**Bottom sheet** (`BackgroundColor=White`, `CornerRadius` top corners only via a `Frame` with `CornerRadius="16"`, negative top margin `-24` to overlap):
- Title: `"Connexion"`, `FontSize=18`, `FontAttributes=Bold`
- Email field with `<Entry Placeholder="Adresse e-mail" Keyboard="Email"/>`
- Password field with `<Entry Placeholder="Mot de passe" IsPassword="True"/>` + toggle visibility button (eye icon)
- Primary button: `"Se connecter"`, style `PrimaryButtonStyle`
- Forgot password: `<Label Text="Mot de passe oublié ?" TextColor="{StaticResource Primary}" HorizontalOptions="Center"/>`

---

### 2. `Views/Dashboard/DashboardPage.xaml`

**Delegue view:**

Header (`BackgroundColor=Primary`, `Padding="16,48,16,64"`):
- Row: Avatar circle (initials, 40×40, white semi-transparent bg) + welcome text column + bell icon
- Below: date label in white muted

KPI cards row (overlapping header by `-28` margin, `ZIndex` trick via `Grid`):
- Two `Frame` cards side-by-side (`CardStyle`), each showing: colored icon + big number + label
- Card 1: visits today (icon `📍`, color Primary)
- Card 2: visits done (icon `✅`, color Secondary)

Quick access section:
- Title: `"Accès rapide"` (`SectionTitleStyle`)
- Two buttons in a `Grid` 2-col: "Mes visites" (filled Primary) + "Planning" (outline/surface)

Objectives section:
- Title: `"Mes objectifs du mois"`
- For each objective: label + value right-aligned + `ProgressBar` below

---

### 3. `Views/Planning/PlanningPage.xaml`

Header (`BackgroundColor=Primary`):
- Row: left arrow + `"Planning — Semaine XX"` centered + right arrow
- Subtitle with week date range

Week day picker (`CollectionView` horizontal or manual `Grid` 7-col):
- Each day: letter abbreviation + day number
- Selected day: `BackgroundColor=Primary`, `TextColor=White`, `CornerRadius=8`
- Other days: transparent background, `TextColor=TextSecondary`

Planning list (filtered by selected day):
- Each item: `Frame` with left `BoxView` accent bar (3px wide, `BackgroundColor` by status color)
- Inside: client name (bold) + time range + status badge
- Status colors: `PLANIFIEE`=Secondary, `REALISEE`=Primary, `ANNULEE`=Danger

FAB (Floating action): `"+ Ajouter une visite"` button at bottom, `BackgroundColor=Primary`, full width, `CornerRadius=10`

---

### 4. `Views/Visites/VisitListPage.xaml`

Header (`BackgroundColor=Primary`):
- Title `"Mes visites"`
- Search bar below: `BackgroundColor` white 15% alpha, `TextColor=White`, `PlaceholderColor` white 60%

Filter chips row (`ScrollView` horizontal, `Padding="14,10"`):
- Active chip: `BackgroundColor=Primary`, `TextColor=White`, `CornerRadius=20`
- Inactive chip: `BackgroundColor=SurfaceBackground`, `TextColor=TextSecondary`, border `BorderColor`

Visit list items (inside `CollectionView`):
- `Frame` (`CardStyle`) for each item
- Row 1: client name (bold, 14pt) + status badge right-aligned
- Row 2: date + time (muted, 12pt)
- Separator line + action buttons row: "Créer rapport" (Primary filled, flex 1) + "Détail" (surface, fixed width)

Empty state: centered icon + `"Aucune visite trouvée"` muted italic label

FAB: `"+ Nouvelle visite"` dashed border button at bottom

---

### 5. `Views/Rapports/RapportPage.xaml`

Header (`BackgroundColor=Primary`):
- Back arrow + `"Rapport de visite"` title
- Subtitle: client name + date (white muted)

Form fields (inside `ScrollView`, `Padding="14"`):

**Content field:**
- Label: `"Contenu du rapport *"` (12pt, muted)
- `Editor` with `BackgroundColor=SurfaceBackground`, `MinimumHeightRequest=100`, `CornerRadius=10`
- Helper: `"Minimum 20 caractères"` (11pt, muted)

**Result selector:**
- Label: `"Résultat *"`
- Three `Frame` buttons side-by-side in a `Grid` 3-col:
  - Selected: `BackgroundColor=Primary`, `TextColor=White`
  - Unselected: `BackgroundColor=SurfaceBackground`, border
  - Labels: "Positif" / "Négatif" / "En attente"

**Products discussed:**
- Label: `"Produits discutés"`
- `CollectionView` of checkboxes — each row: colored checkbox square + product name
- Checked: `BackgroundColor=Primary` checkbox, `BorderColor=Primary` on row frame
- Unchecked: white checkbox with border

**GPS indicator:**
- `Frame` with `BackgroundColor=PrimaryLight`, `BorderColor` none
- Row: pin icon (Primary color) + status text column
- States: `"Localisation en cours…"` (muted) / `"Position capturée ✓"` (PrimaryText, bold) / `"Permission refusée"` (Danger)

**Submit button:** `PrimaryButtonStyle`, `Text="Soumettre le rapport"`, full width, bottom

---

### 6. `Views/Products/ProductListPage.xaml`

Header (`BackgroundColor=Primary`):
- Title `"Catalogue"`
- Search bar (same style as visit list)
- Offline banner (visible only when offline): yellow strip `BackgroundColor=SecondaryLight`, `TextColor=SecondaryText`, `"⚠️ Mode hors ligne — catalogue du dernier chargement"`

Category chips: horizontal `ScrollView` with filter chips (same chip style as visits)

Product list items:
- `Frame` (`CardStyle`), horizontal layout
- Product image/icon box (52×52, `CornerRadius=10`, category-colored background)
- Name (bold 14pt) + category (muted 12pt) + price row
- If promo active: original price with `TextDecorations=Strikethrough` (muted) + discounted price (Primary, bold) + promo badge (`"-15%"`, `BackgroundColor=SecondaryLight`, `TextColor=SecondaryText`)

---

### 7. `Views/Orders/CreateOrderPage.xaml`

**Wizard progress bar at top** (inside header or just below):
- Three segments `BoxView` in `Grid` 3-col
- Active step: `BackgroundColor=White`
- Inactive steps: `BackgroundColor` white at 30% alpha
- Step labels below in white/muted white

**Step 1 — Cart:**
- Cart items list: each item in `Frame` (`CardStyle`)
  - Product name + promo badge (if applicable)
  - Price row: original struck-through + discounted price (Primary)
  - Quantity stepper: minus button + number + plus button
  - Line total right-aligned
- Order summary `Frame` (`BackgroundColor=SurfaceBackground`):
  - Subtotal row
  - Promo savings row (PrimaryText color, bold)
  - Divider
  - Total row (Primary, large)
- Next button: `PrimaryButtonStyle`, `"Suivant — Notes de livraison"`

**Step 3 — Confirmation:**
- Centered success icon (64×64 circle, `BackgroundColor=PrimaryLight`, checkmark)
- `"Commande confirmée !"` title
- Order number subtitle (muted)
- Summary card: client + total
- Back to orders button (`SecondaryButtonStyle`)

---

### 8. `Views/Stock/MyStockPage.xaml`

Header: `BackgroundColor=Primary`, title `"Mon stock"`

Tab bar (below header, inside white area):
- Two tabs: `"Échantillons"` / `"Promotionnels"`
- Active tab: bottom border `2px Primary`, `TextColor=Primary`, bold
- Inactive tab: `TextColor=TextSecondary`

**Échantillons tab:**
- Each item in `Frame` (`CardStyle`):
  - Row 1: product name (bold) + `"X / Y"` count right-aligned
  - Row 2: expiry date (muted 12pt)
  - `ProgressBar` (`ProgressColor=Primary` if >30%, `SecondaryColor` if <30%)
  - `"Distribuer"` button (`PrimaryButtonStyle`, only enabled when `QuantiteRestante > 0`)

**Success snackbar** (post-distribution):
- `Frame` at bottom: `BackgroundColor=PrimaryLight`, `BorderColor=Primary`
- Row: checkmark icon + `"Distribution enregistrée ✓"` (PrimaryText, bold)
- Auto-dismiss after 3 seconds

---

### 9. `Views/Objectifs/ObjectifPage.xaml`

Header: `BackgroundColor=Primary`, title `"Mes objectifs"`

Global achievement card (inside header or overlapping):
- `Frame` `BackgroundColor` white 15% alpha
- Label: `"Taux global"` (white muted)
- Big number: `"60%"` (white, 30pt, bold)

Objectives list:
- Each `Frame` (`CardStyle`):
  - Row: objective type label (bold) + period badge right-aligned
  - `ProgressBar` with color by achievement level:
    - ≥80%: `ProgressColor=Primary`
    - 50–79%: `ProgressColor=Secondary`
    - <50%: `ProgressColor=Danger`
  - Values row: current / target (muted 12pt) + percentage right-aligned (colored, bold)

---

### 10. `Views/Profile/ProfilePage.xaml`

Header (`BackgroundColor=Primary`, taller, `Padding="16,48,16,28"`):
- Centered avatar circle (72×72, white semi-transparent bg, initials 24pt bold white)
- Name (white, 16pt bold)
- Role badge (white 20% bg, white text, pill shape)

Profile fields (each in a row with border-bottom separator):
- Icon (18pt, TextSecondary) + label column (muted 12pt above, value 14pt below)
- Fields: email, phone, region/address

Action buttons:
- `"Changer le mot de passe"` → `SecondaryButtonStyle` with lock icon
- `"Se déconnecter"` → `Frame` `BackgroundColor=DangerLight`, `BorderColor=#F7C1C1`, text `TextColor=DangerText`, bold, with logout icon

---

## General Rules (apply to ALL pages)

1. **No inline hardcoded colors** — always use `StaticResource` keys defined in `Colors.xaml`
2. **No shadows** — set `HasShadow="False"` on all `Frame` elements; use borders instead
3. **Consistent spacing** — page padding `16px` horizontal, card padding `14px`, gap between cards `10px`
4. **Font sizes** — page title `18pt`, section title `14pt`, body `14pt`, secondary `12pt`, hint `11pt`; never go below `11pt`
5. **CornerRadius** — cards `12`, buttons `10`, chips `20`, badges `12`, small elements `8`
6. **All buttons 44px minimum height** for touch targets
7. **CollectionView over ListView** everywhere — better performance on MAUI
8. **Header always `BackgroundColor=Primary`** — no exceptions, every page must have the green header
9. **Status bar color** — set `Shell.StatusBarColor="{StaticResource Primary}"` in `AppShell.xaml`
10. **Empty states** — every list page must have an empty state: centered icon + muted italic message

---

## File Execution Order

Apply changes in this order to avoid missing resource errors:

1. `Resources/Styles/Colors.xaml` — define all color keys
2. `Resources/Styles/Styles.xaml` — define all global styles
3. `Views/Auth/LoginPage.xaml`
4. `Views/Dashboard/DashboardPage.xaml`
5. `Views/Planning/PlanningPage.xaml`
6. `Views/Visites/VisitListPage.xaml`
7. `Views/Visites/VisitDetailPage.xaml`
8. `Views/Rapports/RapportPage.xaml`
9. `Views/Products/ProductListPage.xaml`
10. `Views/Products/ProductDetailPage.xaml`
11. `Views/Orders/OrderListPage.xaml`
12. `Views/Orders/CreateOrderPage.xaml`
13. `Views/Orders/OrderDetailPage.xaml`
14. `Views/Stock/MyStockPage.xaml`
15. `Views/Objectifs/ObjectifPage.xaml`
16. `Views/Profile/ProfilePage.xaml`
17. `Views/Documents/DocumentListPage.xaml`
18. `Views/Documents/DocumentDetailPage.xaml`
19. `AppShell.xaml` — flyout menu styling

---

## What NOT to change

- Do NOT modify any `.cs` files (ViewModels, Services, Models, code-behind logic)
- Do NOT change data bindings, command names, or property names
- Do NOT remove any existing `x:Name` attributes
- Do NOT change navigation routes
- Do NOT alter `MauiProgram.cs` or `App.xaml.cs`
- Only modify XAML layout, colors, styles, and visual structure
