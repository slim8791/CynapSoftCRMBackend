# TERRAIN_FIXES_APPLIED — Module Terrain

Date: 2026-05-26

## Backend — FieldAPI

### 1. Region.CodePostal: int → string
- **Files**: `CynapCRM.Services.FieldAPI/Models/Region.cs`, `Models/Dto/RegionDto.cs`
- **Fix**: Changed `public int CodePostal` to `public string CodePostal = string.Empty` in both files.
- **Why**: Angular's `RegionDto` interface and MAUI's `Region.cs` both declare `codePostal` as string (e.g. "75001"). Backend was silently rejecting or truncating postal codes sent from the frontend.

### 2. EF Core Migration
- **Migration**: `20260526140241_ChangeCodePostalToString`
- **Command**: `dotnet ef migrations add ChangeCodePostalToString && dotnet ef database update`
- Applied to FieldAPI SQL Server database — column `Regions.CodePostal` converted from `int` to `nvarchar(450)`.

---

## Angular — Cynapharm

### 3. planning-list: Add "Valider" button
- **Files**: `planning-list.component.ts`, `planning-list.component.html`
- **Fix**: Added `validatePlanning(id, etat)` method that calls `PlanningService.validate(id)`. Added "Valider" button visible only for ADMIN/SUPERVISEUR, disabled when `etat === EtatPlanning.Confirme`.

### 4. objectif-list: Fix Periode display
- **Files**: `objectif-list.component.ts`, `objectif-list.component.html`
- **Fix**: Added `periodeLabel(p: number): string` method mapping `PeriodeObjectif` enum values (1/2/3) to "Mensuel"/"Trimestriel"/"Annuel". Template updated from `{{ o.periode }}` to `{{ periodeLabel(o.periode) }}`.

### 5. kpi-dashboard: Add PerformanceDto[] progress bars
- **Files**: `kpi-dashboard.component.ts`, `kpi-dashboard.component.html`
- **Fix**: Added `performances: any[]` field. In `load()`, calls `KpiService.getPerformance(id)` and stores result. HTML now renders performance cards with type label, percentage, and CSS progress bar using `[style.width.%]="p.pourcentage"`.

---

## MAUI — Cynapharm-Mobile

### 6. Rapport.cs: Remove ProduitsDiscutes field
- **File**: `Models/Field/Rapport.cs`
- **Fix**: Removed `public string? ProduitsDiscutes { get; internal set; }`. This field had no corresponding backend property and was dead weight.

### 7. RapportViewModel.cs: Remove ProduitsDiscutes usage
- **File**: `ViewModels/Rapports/RapportViewModel.cs`
- **Fix**: Removed `ProduitsDiscutes = selectedIds.Count > 0 ? JsonSerializer.Serialize(selectedIds) : null` from `Rapport` object construction in `SubmitAsync`. Removed `ProduitsDiscutes = rapport.ProduitsDiscutes` from `PendingRapportEntry` insertion. Removed unused `using System.Text.Json`.

### 8. SyncService.cs: Remove ProduitsDiscutes usage
- **File**: `Services/SyncService.cs`
- **Fix**: Removed `ProduitsDiscutes = entry.ProduitsDiscutes` from `Rapport` object construction in `FlushPendingRapportsAsync`.

### 9. VisitDetailPage.xaml: Replace old form fields with Type picker
- **File**: `Views/Visites/VisitDetailPage.xaml`
- **Fix**: Removed Client (Entry), Statut (Picker), Notes (Editor) fields. Added:
  - **Type de visite** Picker (Médecin/Pharmacien/Autre) bound to `SelectedTypeLabel`
  - **ID Médecin** optional numeric Entry bound to `SelectedMedecinId`
  - **ID Pharmacien** optional numeric Entry bound to `SelectedPharmacienId`

### 10. VisitDetailViewModel.cs: Add VisiteTypeOptions / SelectedTypeLabel
- **File**: `ViewModels/Visites/VisitDetailViewModel.cs`
- **Fix**: Added `VisiteTypeOptions` list and `SelectedTypeLabel` get/set property that converts between `SelectedType` (int 1/2/3) and display strings. Added `partial void OnSelectedTypeChanged` to notify `SelectedTypeLabel` on change.

### 11. DashboardViewModel.cs: Remove dead KpiItems collection
- **File**: `ViewModels/Dashboard/DashboardViewModel.cs`
- **Fix**: Removed `ObservableCollection<Kpi> KpiItems` which referenced a non-existent `Kpi` type causing build error CS0246.

### 12. DashboardPage.xaml: Replace KpiItems with PerformanceItems
- **File**: `Views/Dashboard/DashboardPage.xaml`
- **Fix**: Replaced `BindableLayout.ItemsSource="{Binding KpiItems}"` and `x:DataType="field:Kpi"` template with `PerformanceItems` using `PerformanceDto`. Cards now show `TypeLabel`, `Pourcentage`, and progress bar.

---

## Build Results

- `CynapCRM.Services.FieldAPI`: **0 errors**, 4 warnings (pre-existing: duplicate PackageReference, AutoMapper vulnerability)
- `Cynapharm-Mobile`: **0 errors**, 17 warnings (pre-existing: CommunityToolkit.Maui version constraint, XML doc comments)
