# DÉLÉGUÉ — Analyse complète du scénario terrain
# Planning → Visite → Rapport

**Date :** 2026-05-28  
**Branche :** `dev/Mobile-0001`  
**Périmètre :** Scénario terrain du DÉLÉGUÉ dans l'app MAUI Cynapharm-Mobile

---

## ÉTAPE 1 — PlanningPage

### 1.1 Navigation vers PlanningPage

- **Route Shell absolute :** `//planning`  
- **Wired in AppShell.xaml.cs :** `GoToPlanningCommand → Navigate("//planning")`  
- **Visible pour :** rôle DÉLÉGUÉ uniquement (`ShowPlanning = isDelegue` dans `ApplyRoleVisibility`)  
- **Sur OnAppearing :** `PlanningPage.cs` appelle `vm.LoadWeekCommand.ExecuteAsync(null)` à chaque apparition de la page.

```csharp
// PlanningPage.xaml.cs
protected override void OnAppearing()
{
    base.OnAppearing();
    if (BindingContext is PlanningViewModel vm) _ = vm.LoadWeekCommand.ExecuteAsync(null);
}
```

> ⚠️ **Double-load potentiel** : `OnWeekStartChanged` déclenche aussi `LoadWeekAsync` dès que `WeekStart` change (y compris à l'initialisation du ViewModel). Sur la première ouverture, deux chargements peuvent s'exécuter en parallèle.

---

### 1.2 Affichage de la semaine

**Nombre de jours affichés :** **6 jours (Lundi → Samedi)**

```csharp
// PlanningViewModel.cs
[ObservableProperty]
private DateTime _weekStart = DateTime.Today.AddDays(
    -(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
```

`WeekStart` est toujours le **lundi le plus récent** (basé sur `DayOfWeek.Monday`).

**WeekLabel :**
```csharp
public string WeekLabel => $"{WeekStart:dd MMM} – {WeekStart.AddDays(6):dd MMM yyyy}";
```
> ⚠️ **Incohérence label/affichage :** Le label affiche une plage de **7 jours** (lundi → dimanche, `AddDays(6)`), mais la boucle n'affiche que **6 jours** (i < 6 → lundi → samedi). Le label affiche par exemple "02 Juin – 08 Juin" alors que le dimanche 08 Juin ne figure pas dans la liste.

**Navigation semaine précédente / suivante :**
```csharp
[RelayCommand] private void PreviousWeek() {
    var candidate = WeekStart.AddDays(-7);
    if (candidate >= DateTime.Today.AddYears(-1)) WeekStart = candidate;  // limité à -1 an
}
[RelayCommand] private void NextWeek() => WeekStart = WeekStart.AddDays(7);
```

**Chargement des planning items :**
- Service : `PlanningService.GetPlanningAsync(weekStart)`
- Endpoint : `GET fields/plannings/by-range?idDelegue={userId}&startDate={lundi}&endDate={lundi+6}`
- `endDate = weekStart.AddDays(6)` → **7 jours de données récupérées** (lundi → dimanche) mais **seuls 6 sont affichés** (lundi → samedi). Le dimanche est téléchargé mais jamais affiché.
- Filtrage local : `entries.Where(e => e.DatePlanifiee.Date == day.Date)`

**Champs affichés sur chaque carte planning :**

| Champ | Affiché | Source |
|-------|---------|--------|
| `ClientNom` | ✅ Oui | `Planning.ClientNom` (champ local, non chargé depuis l'API de liste) |
| `TimeRange` | ✅ Oui | Calculé : `{HeureDebut:hh\:mm} – {HeureFin:hh\:mm}` |
| Badge Etat | ✅ Oui | `EtatToColorConverter` + `EtatToTextConverter` sur `Planning.Etat` (int) |
| Date | ❌ Non | Visible seulement via le groupe jour |
| Nom du délégué | ❌ Non | — |

**Badge Etat :**
- ✅ Présent — ajouté via `EtatToColorConverter` et `EtatToTextConverter`
- `0` → "En attente" (amber `#FF8F00`) / `1` → "Confirmé" (vert `#2E7D32`) / `2` → "Annulé" (rouge `#C62828`)

**Samedi affiché :** ✅ Oui (boucle `i < 6`, Mon→Sat).

> ⚠️ **`ClientNom` jamais rempli depuis l'API** : `Planning.ClientNom` est défini comme `public string ClientNom { get; set; } = string.Empty;` sans `[JsonPropertyName]`. Le backend retourne les données de planning sans ce champ — les cartes afficheront un nom vide.

---

### 1.3 Bouton "+" par jour

```csharp
// PlanningViewModel.cs
[RelayCommand]
private async Task AddVisitAsync(DateTime date)
{
    var dateStr = date != default
        ? date.ToString("yyyy-MM-dd")
        : DateTime.Today.ToString("yyyy-MM-dd");
    await Shell.Current.GoToAsync($"//visits/detail?prefillDate={dateStr}");
}
```

- Navigate vers `//visits/detail?prefillDate=YYYY-MM-DD`
- Passe : **date du jour sélectionné** uniquement
- ❌ **Ne passe PAS `idPlanning`** : L'association planning → visite est perdue. `SelectedPlanningId` dans `VisitDetailViewModel` restera `null` et la visite créée ne sera pas liée au planning.

---

### 1.4 Bouton "+ Ajouter une visite" (bas de page)

```csharp
// XAML PlanningPage.xaml — commande sticky bottom button
Command="{Binding AddVisitCommand}"
// Sans CommandParameter → date = default(DateTime)
```

```csharp
// PlanningViewModel.cs — AddVisitAsync
var dateStr = date != default
    ? date.ToString("yyyy-MM-dd")
    : DateTime.Today.ToString("yyyy-MM-dd");   // fallback = aujourd'hui
```

- Navigate vers `//visits/detail?prefillDate=AUJOURD'HUI`
- Pas de date spécifique (aujourd'hui par défaut)
- ❌ Également sans `idPlanning`

---

### 1.5 Known issues in PlanningPage

| # | Issue | Impact |
|---|-------|--------|
| P1 | `WeekLabel` affiche `AddDays(6)` (dimanche) mais boucle `i < 6` = samedi → label trompeur | Faible |
| P2 | API fetche 7 jours (`endDate = AddDays(6)`) mais UI n'en affiche que 6 → données dimanche gaspillées | Faible |
| P3 | Bouton "+" par jour ne passe PAS `idPlanning` → liaison planning-visite perdue | **Élevé** |
| P4 | Bouton sticky "+ Ajouter" ne passe PAS `idPlanning` (même problème) | **Élevé** |
| P5 | `Planning.ClientNom` jamais rempli depuis l'API de liste → cartes avec nom vide | Moyen |
| P6 | Double-load potentiel : `OnAppearing` + `OnWeekStartChanged` peuvent déclencher deux `LoadWeekAsync` simultanément | Moyen |

---

## ÉTAPE 2 — VisiteDetailPage (création)

### 2.1 Navigation from PlanningPage

- **Route :** `//visits/detail` (route absolue enregistrée dans `AppShell.xaml.cs`)
- **Query params reçus :**

| Param | Property VM | QueryProperty |
|-------|-------------|---------------|
| `visiteId` | `VisiteId` (int) | `[QueryProperty(nameof(VisiteId), "visiteId")]` |
| `prefillDate` | `PrefillDate` (string) | `[QueryProperty(nameof(PrefillDate), "prefillDate")]` |
| `idPlanning` | `IdPlanningRaw` (string) | `[QueryProperty(nameof(IdPlanningRaw), "idPlanning")]` |

**Application dans le ViewModel :**

```csharp
partial void OnPrefillDateChanged(string value)
{
    if (DateTime.TryParse(value, out var dt)) VisiteDate = dt;
}
```

- `prefillDate` → pré-rempli `VisiteDate` ✅
- `idPlanning` → dans `InitAsync` : `int.TryParse(IdPlanningRaw, out var pid)` → appel API planning pour label ✅  
- ❌ `idPlanning` n'est **jamais passé depuis PlanningPage** (voir P3/P4)

**Déclenchement de `InitAsync` :**

```csharp
// VisitDetailPage.xaml.cs
protected override void OnNavigatedTo(NavigatedToEventArgs args)
{
    base.OnNavigatedTo(args);
    if (BindingContext is VisitDetailViewModel vm)
        _ = vm.InitCommand.ExecuteAsync(null);
}
```

Utilise `OnNavigatedTo` (pas `OnAppearing`) → déclenché après que les QueryProperties soient injectées. ✅

---

### 2.2 Form fields shown

| Champ | Type MAUI | Propriété VM | Requis | Source |
|-------|-----------|--------------|--------|--------|
| Date de la visite | `DatePicker` | `VisiteDate` (DateTime) | ✅ | Défaut : `DateTime.Now` |
| Type de visite | `Picker` (strings) | `SelectedTypeLabel` → `SelectedType` (int) | ✅ | Liste statique : Médecin / Pharmacien / Autre |
| Notes | `Editor` | `Notes` (string) | ❌ optionnel | Saisi libre |
| Médecin | `Picker` (UserPickerItem) | `SelectedMedecin` | ❌ optionnel | API : `auth/users/by-role/MEDECIN` |
| Pharmacien | `Picker` (UserPickerItem) | `SelectedPharmacien` | ❌ optionnel | API : `auth/users/by-role/CLIENT` ⚠️ |

**Notes :**
- `IsEnabled="{Binding CanEdit}"` sur tous les champs → lecture seule si `IsCompleted=true` ✅
- Pas de champ `ClientNom` libre — le nom est résolu via les Pickers ✅

---

### 2.3 Médecin and Pharmacien dropdowns

**Chargement dans `InitAsync` :**

```csharp
var medecinTask    = _userSvc.GetUsersByRoleAsync("MEDECIN");
var pharmacienTask = _userSvc.GetUsersByRoleAsync("CLIENT");   // ← ⚠️ MAUVAIS RÔLE
await Task.WhenAll(medecinTask, pharmacienTask);
```

- Médecins : endpoint `auth/users/by-role/MEDECIN` ✅
- Pharmaciens : endpoint `auth/users/by-role/CLIENT` ❌  
  → Le Picker "Pharmacien" chargera les utilisateurs ayant le rôle CLIENT, pas PHARMACIEN.

**Pré-remplissage en mode édition :**

```csharp
SelectedMedecin    = Medecins.FirstOrDefault(m => m.Id == visite.IdMedecin);
SelectedPharmacien = Pharmaciens.FirstOrDefault(p => p.Id == visite.IdPharmacien);
```

- ✅ Fonctionne en logique — mais si les listes sont vides (erreur API), les pickers restent `null`.

---

### 2.4 IdPlanning association

- `IdPlanning` query param : **enregistré** (`[QueryProperty]`) mais **jamais passé** par PlanningPage (bug P3)
- Affiché via le banner `PlanningLabel` : ✅ (si `IdPlanningRaw` est reçu, requête `GetPlanningByIdAsync` pour label)
- Inclus dans `CreateVisiteDto.IdPlanning` : ✅

```csharp
var dto = new CreateVisiteDto
{
    ...
    IdPlanning = SelectedPlanningId,   // ← null si non passé
    ...
};
```

---

### 2.5 IdDelegue injection

```csharp
// SaveAsync
var userId = await SecureStorage.GetAsync(StorageKeys.UserId);
var dto = new CreateVisiteDto
{
    ...
    IdDelegue = int.Parse(userId ?? "0"),   // ← ⚠️ RISQUE
};
```

> ⚠️ **Problème** : Si `SecureStorage.GetAsync` retourne `null` (session expirée, problème de chiffrement Android), `int.Parse("0")` silencieusement met `IdDelegue = 0`. Le backend enregistre la visite avec un délégué invalide.  
> **Recommandation :** Valider `userId > 0` avant POST, comme dans `RapportViewModel.SubmitAsync` (correction appliquée dans la dernière session).

---

### 2.6 Payload sent to backend

```csharp
// CreateVisiteDto — champs envoyés à POST fields/visites
{
    "idVisite":     0,                    // 0 pour create, >0 pour update
    "dateVisite":   "2026-06-02T...",
    "type":         1,                    // 1=Médecin, 2=Pharmacien, 3=Autre
    "idMedecin":    3,                    // null si non sélectionné
    "idPharmacien": null,                 // null si non sélectionné
    "idPlanning":   null,                 // null car non passé depuis PlanningPage
    "id_User_Delegue": 7
}
```

**Route API :**
- Création : `POST fields/visites` via `VisiteService.CreateVisiteAsync(dto)`
- Mise à jour : `PUT fields/visites/{id}` via `VisiteService.UpdateVisiteAsync(id, dto)`

---

### 2.7 After save

```csharp
// SaveAsync — fin
_isDirty = false;
HapticService.Success();
await Shell.Current.GoToAsync("..");
```

- Navigation : `GoToAsync("..")` → **retourne à la page précédente** (PlanningPage ou VisiteListPage)
- ❌ **La réponse API (`Visite?`) est ignorée** — l'`id` de la visite créée n'est pas récupéré
- ❌ L'utilisateur ne navigue pas vers la visite créée pour soumettre un rapport — il doit la retrouver manuellement dans la liste

---

### 2.8 Known issues in VisiteDetailPage

| # | Issue | Impact |
|---|-------|--------|
| V1 | `GetUsersByRoleAsync("CLIENT")` pour les pharmaciens → mauvais rôle | **Élevé** |
| V2 | `int.Parse(userId ?? "0")` → IdDelegue silencieusement 0 si session expirée | **Élevé** |
| V3 | Après save : `GoToAsync("..")` → retour PlanningPage/VisitList, pas vers la visite créée | Moyen |
| V4 | `IdPlanning` jamais passé depuis PlanningPage → association planning-visite perdue | **Élevé** |
| V5 | Bouton "Soumettre un rapport" : `IsVisible="{Binding IsExisting}"` → visible même si `IsCompleted=true` | Moyen |
| V6 | Réponse API `Visite?` ignorée après création → `VisiteId` non récupéré | Moyen |
| V7 | `StatutOptions = { "PLANIFIEE", "REALISEE", "ANNULEE" }` défini mais le Picker Statut a été **supprimé** du XAML (FIX-3) — `_statut` reste dans le ViewModel inutilement | Faible |

---

## ÉTAPE 3 — VisiteListPage

### 3.1 What is shown per visite card

```
Carte visite :
├── Barre colorée gauche (5px)
│     IsCompleted=false → orange #FF9800
│     IsCompleted=true  → vert #4CAF50
├── Colonne gauche (VerticalStackLayout)
│     Ligne 1 : 📅 {DateVisite:dd/MM/yyyy} (gras)
│     Ligne 2 : Badge TypeLabel (chip Primary-light)
│                "Médecin" | "Pharmacien" | "Autre"
│     Ligne 3 : ContactName (si HasContact=true)
│                = MedecinNom ou PharmacienNom (résolu API)
├── Colonne droite : Badge statut
│     IsCompleted=false → "Non complétée" (fond orange #FFF3E0)
│     IsCompleted=true  → "Complétée" (fond vert #E8F5E9)
└── Flèche › (TextColor Gray300)
```

| Champ | Format | Source |
|-------|--------|--------|
| Date | `dd/MM/yyyy` (sans heure) | `Visite.DateVisite` |
| Type | Label ("Médecin" / "Pharmacien" / "Autre") | `Visite.TypeLabel` (computed, `[JsonIgnore]`) |
| Nom contact | Résolu après chargement | `Visite.MedecinNom` / `PharmacienNom` (set par VM) |
| Statut | "Complétée" / "Non complétée" | `DataTrigger` sur `Visite.IsCompleted` |

---

### 3.2 Filters

**Filtre date (DatePicker × 2) :**
- `FilterStartDate` : défaut `DateTime.Today.AddDays(-30)`
- `FilterEndDate` : défaut `DateTime.Today`
- Filtrage côté **client** (post-API) dans `GetVisitesAsync`
- ✅ Fonctionnel

**Filtre statut (chips) :**
- Options : `{ "Tous", "Non complétée", "Complétée" }`
- Correspondance : `"Complétée"` → `v.IsCompleted == true` / `"Non complétée"` → `v.IsCompleted == false`
- ✅ Cohérent avec les badges de la carte

**Recherche texte :**
```csharp
filtered = filtered.Where(v =>
    v.TypeLabel.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
```
- ✅ Fonctionne sur le TypeLabel  
- ❌ **Ne recherche pas sur le nom du contact** (MedecinNom, PharmacienNom) — le champ le plus naturel pour un délégué

---

### 3.3 Navigation to detail

```csharp
// VisitListViewModel.GoToDetailAsync
await Shell.Current.GoToAsync($"//visits/detail?visiteId={visite.Id}");
```

- Route absolue `//visits/detail`
- Passe `visiteId` ✅
- VisiteDetailPage reçoit via `[QueryProperty]` et charge dans `InitAsync` ✅

---

### 3.4 Known issues in VisiteListPage

| # | Issue | Impact |
|---|-------|--------|
| L1 | Recherche uniquement sur TypeLabel, pas sur le nom du contact | Moyen |
| L2 | 2 requêtes API supplémentaires (MEDECIN + CLIENT) à chaque chargement pour résoudre les noms | Faible (perf) |
| L3 | Si l'API utilisateurs est en erreur, les noms restent vides (swallowed catch) — pas de feedback | Faible |
| L4 | `GetUsersByRoleAsync("CLIENT")` pour les pharmaciens (même bug que V1) — noms de pharmaciens jamais résolus | **Élevé** |

---

## ÉTAPE 4 — VisiteDetailPage (visite existante)

### 4.1 Fields shown in read/edit mode

**CanEdit = `!IsCompleted` :**

| Contrôle | `IsCompleted=false` | `IsCompleted=true` |
|----------|--------------------|--------------------|
| DatePicker | `IsEnabled=True` | `IsEnabled=False` |
| Picker Type | `IsEnabled=True` | `IsEnabled=False` |
| Editor Notes | `IsEnabled=True` | `IsEnabled=False` |
| Picker Médecin | `IsEnabled=True` | `IsEnabled=False` |
| Picker Pharmacien | `IsEnabled=True` | `IsEnabled=False` |
| Bouton "Enregistrer" | ✅ Visible | ❌ Caché (`IsVisible="{Binding CanEdit}"`) |
| Bouton "Supprimer" | ✅ Visible (`CanDelete=true`) | ❌ Caché (`CanDelete=false`) |
| Banner "Visite terminée" | ❌ Caché | ✅ Visible |

✅ Mode lecture-seule correctement implémenté.

---

### 4.2 Button "Soumettre un rapport"

```xml
<!-- VisitDetailPage.xaml — ligne 193-221 -->
<Border ...
        IsVisible="{Binding IsExisting}">
    <Label Text="Soumettre un rapport" .../>
    <Border.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding GoToRapportCommand}" />
    </Border.GestureRecognizers>
</Border>
```

```csharp
// VisitDetailViewModel
[RelayCommand]
private async Task GoToRapportAsync()
    => await Shell.Current.GoToAsync($"//visits/rapport?visiteId={VisiteId}");
```

- **Visible quand :** `IsExisting=true` (VisiteId > 0) — **toujours visible pour une visite existante**
- ❌ **Également visible quand `IsCompleted=true`** — or, une visite terminée a déjà un rapport ; le bouton ne devrait pas apparaître. Le backend rejette la requête (400) mais l'UX est cassée.
- Route : `//visits/rapport?visiteId={VisiteId}` ✅
- Param passé : `visiteId` ✅

---

### 4.3 Known issues (existing visite)

| # | Issue | Impact |
|---|-------|--------|
| E1 | Bouton "Soumettre rapport" visible même quand `IsCompleted=true` | Moyen |
| E2 | Après soumission du rapport, retour à VisiteDetailPage — `IsCompleted` pas mis à jour localement → UI toujours éditable | Moyen |

---

## ÉTAPE 5 — RapportPage

### 5.1 Navigation to RapportPage

```
Route enregistrée : Routing.RegisterRoute("visits/rapport", typeof(RapportPage));
Appelée depuis VisiteDetailPage : //visits/rapport?visiteId={VisiteId}
```

- `[QueryProperty(nameof(LinkedVisiteId), "visiteId")]` reçoit `visiteId` ✅
- `partial void OnLinkedVisiteIdChanged(int value)` → déclenche `LoadProduitsAsync()` si value > 0 ✅
- `RapportPage.xaml.cs.OnAppearing` : déclenche `LoadProduitsCommand` ET `PreCaptureLocationCommand`

> ⚠️ **Double LoadProduits possible** : `OnLinkedVisiteIdChanged` est appelé quand la QueryProperty est injectée (avant `OnAppearing`), puis `OnAppearing` le redéclenche. Les deux exécutions sont quasi simultanées.

---

### 5.2 GPS flow — description complète

**Flux en deux phases :**

**Phase 1 — OnAppearing (rapide, sans dialog) :**
```csharp
// RapportPage.xaml.cs
_ = vm.PreCaptureLocationCommand.ExecuteAsync(null);
```
```csharp
// PreCaptureLocationAsync — GetLastKnownLocationAsync
var last = await Geolocation.GetLastKnownLocationAsync();
if (last != null) {
    GeoStatus = $"📍 Dernière position : {lat:F4}, {lon:F4} (il y a X min)";
    CapturedLatitude  = last.Latitude;
    CapturedLongitude = last.Longitude;
} else {
    GeoStatus = "📍 En attente du signal GPS…";
}
```
- Utilise la **dernière position connue** (cache GPS — peut être vieille de plusieurs heures)
- Aucune dialog de permission
- Si indisponible → `"⚠️ Géolocalisation indisponible"`

**Phase 2 — Sur tap "Soumettre" (précise, avec dialog) :**
```csharp
// SubmitAsync → CaptureLocationAsync
var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
// Si refusée → (null, null), soumission continue
var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
var location = await Geolocation.GetLocationAsync(request);
```

| Situation | Comportement |
|-----------|-------------|
| Permission refusée | `GeoStatus="⚠️ Permission refusée"`, `(null, null)`, **soumission continue** |
| Timeout (10s sans signal) | `location=null`, `GeoStatus="⚠️ Position non disponible"`, **soumission continue** |
| GPS indisponible (hardware) | `FeatureNotSupportedException`, **soumission continue** |
| Succès | Lat/Lon remplis, `GeoStatus="✅ Position capturée"` |

**Où sont stockés Latitude/Longitude :**
- `CapturedLatitude`, `CapturedLongitude` dans le ViewModel (mis à jour par `PreCapture` ET par `CaptureLocationAsync`)
- Valeurs finales définies dans `SubmitAsync` :
  ```csharp
  var (lat, lon) = await CaptureLocationAsync();
  CapturedLatitude  = lat;
  CapturedLongitude = lon;
  rapport.Latitude  = lat;
  rapport.Longitude = lon;
  ```

> ⚠️ **Problème UX GPS :** La permission de localisation n'est demandée qu'**à la soumission**, pas à l'ouverture de la page. L'utilisateur attend 10 secondes pour la capture GPS **après avoir tout rempli**. Mauvaise expérience utilisateur.

> ⚠️ **Pas d'avertissement "Activez le GPS" :** Si le GPS est désactivé dans les paramètres Android, aucun message n'indique à l'utilisateur d'activer la localisation avant de remplir le formulaire.

---

### 5.3 Form fields

| Champ | Type | Propriété VM | Validation | Options |
|-------|------|--------------|------------|---------|
| Résultat | `Picker` | `SelectedResultat` (défaut "POSITIF") | ✅ requis (non vide) | "POSITIF", "NÉGATIF", "À RELANCER", "SANS SUITE" |
| Contenu | `Editor` | `Contenu` | ✅ `[Required]` + `[MinLength(20)]` | min 20 chars |
| Compteur chars | `Label` | `Contenu.Length` | — | Affiché en bas ✅ |
| Produits discutés | `CollectionView` + `CheckBox` | `ProduitsDiscutes` (ObservableCollection) | ❌ optionnel | Chargés API ou SQLite |

**Produits discutés :**
- Chargés depuis `products/catalog` (actifs, non archivés) — fallback SQLite si hors-ligne ✅
- Sélection via `IsSelected` (CheckBox) ✅
- Serialisés en JSON avant soumission : `[{id, nom}, ...]` ✅
- Envoyés dans le payload `ProduitsDiscutes` ✅
- `HeightRequest="300"` sur le `CollectionView` pour éviter l'erreur de rendu Android ✅

**Ordre d'affichage (FIX-5) :**
1. GPS banner
2. Résultat (Picker — sélection rapide)
3. Contenu du rapport (Editor)
4. Produits discutés (checkboxes)
5. Bouton sticky "Soumettre"

---

### 5.4 Offline behavior

**Si `NetworkAccess != Internet` :**
```csharp
await _localDb.InsertPendingRapportAsync(new PendingRapportEntry
{
    VisiteId            = rapport.VisiteId,
    Contenu             = rapport.Contenu,
    Resultat            = rapport.Resultat,
    ProduitsDiscutes    = produitsJson,       // ✅ inclus
    DateSoumissionTicks = rapport.DateSoumission.Ticks,
    Latitude            = lat,
    Longitude           = lon,
    IsSynced            = false
});
await Shell.Current.DisplayAlert("Enregistré hors ligne", "...", "OK");
await Shell.Current.GoToAsync("..");
```

- ✅ Sauvegardé en SQLite
- ✅ Toast affiché
- ✅ Navigation retour

**SyncService.FlushPendingRapportsAsync :**
```csharp
var rapport = new Rapport
{
    Id             = 0,
    VisiteId       = entry.VisiteId,
    Contenu        = entry.Contenu,
    Resultat       = entry.Resultat,
    DateSoumission = new DateTime(entry.DateSoumissionTicks),
    Latitude       = entry.Latitude,
    Longitude      = entry.Longitude
    // ⚠️ ProduitsDiscutes NON inclus
    // ⚠️ IdDelegue NON explicitement défini (= 0 → fallback SecureStorage dans service)
};
await _visiteService.CreateRapportAsync(rapport);
```

- ✅ Déclenché sur reconnexion (`Connectivity.ConnectivityChanged` dans `App.xaml.cs`)
- ✅ Guard anti-concurrent avec `Interlocked.CompareExchange`
- ❌ **`ProduitsDiscutes` non inclus** lors du flush — les produits discutés sont perdus en mode offline
- ❌ **`IdDelegue` non défini** dans le rapport reconstruit (= 0) — le service tombe en fallback SecureStorage, ce qui fonctionne si l'utilisateur est le même, mais est fragile

---

### 5.5 Payload sent to backend

```csharp
// VisiteService.CreateRapportAsync — payload final
{
    "Id_Rapport":       0,
    "Id_Visite":        {LinkedVisiteId},
    "Commentaire":      "{Contenu}",          // mappé depuis rapport.Contenu
    "Resultat":         "{SelectedResultat}",  // "POSITIF"|"NÉGATIF"|"À RELANCER"|"SANS SUITE"
    "Id_User_Delegue":  {userId},             // depuis rapport.IdDelegue (validé > 0 avant)
    "Latitude":         {lat ou null},
    "Longitude":        {lon ou null},
    "ProduitsDiscutes": "[{\"id\":1,\"nom\":\"...\"}]" ou null
}
```

Route : `POST fields/rapports/createUpdate` (gateway → `POST api/rapports/createUpdate` FieldAPI)

---

### 5.6 After submit

```csharp
await _visiteService.CreateRapportAsync(rapport);
await Shell.Current.GoToAsync("..");   // → retour VisiteDetailPage
```

- ✅ Retour à `VisiteDetailPage`
- ❌ **`IsCompleted` n'est pas mis à jour localement** après soumission — `VisiteDetailPage` reste en mode édition jusqu'au prochain rechargement (`OnNavigatedTo` → `InitAsync`)
- ❌ La **réponse du rapport créé n'est pas utilisée** — impossible de naviguer directement vers le rapport

---

### 5.7 Known issues in RapportPage

| # | Issue | Impact |
|---|-------|--------|
| R1 | GPS permission demandée au moment du tap "Soumettre" → UX bloquée 10 secondes | Moyen |
| R2 | Pas d'avertissement "Activez le GPS" à l'ouverture de la page | Faible |
| R3 | `SyncService` ne ré-inclut pas `ProduitsDiscutes` lors du flush offline → données perdues | **Élevé** |
| R4 | `SyncService` ne définit pas `IdDelegue` → fallback silencieux SecureStorage | Moyen |
| R5 | `_resultat` (ancienne propriété) coexiste avec `_selectedResultat` (nouvelle) → code mort | Faible |
| R6 | Après soumission, `IsCompleted` non mis à jour localement sur VisiteDetailPage | Moyen |
| R7 | Double chargement des produits : `OnLinkedVisiteIdChanged` + `OnAppearing` | Faible |
| R8 | La 400 était causée par : (1) `IdDelegue=0` si SecureStorage nul, (2) `ProduitsDiscutes` absent du payload (corrigé dans dernière session) | Résolu ✅ |

**Cause exacte de la HTTP 400 (résolue) :**
1. `rapport.IdDelegue` n'était pas défini dans `SubmitAsync` → la valeur `0` était envoyée → backend : `visite.Id_User_Delegue != 0` → `return null` → 400
2. `ProduitsDiscutes` absent du payload `VisiteService` malgré son ajout au DTO

---

## PARTIE 6 — NAVIGATION MAP

```
AppShell Flyout
  └── //planning
        [PlanningPage.OnAppearing → LoadWeekCommand]
        |
        ├── tap "+" (jour X)
        |     └── //visits/detail?prefillDate=X
        |           ⚠️ idPlanning NON passé
        |           [VisitDetailPage.OnNavigatedTo → InitAsync]
        |           (form: date pré-remplie, picker médecin/pharmacien chargés)
        |           |
        |           └── tap "Enregistrer"
        |                 → GoToAsync("..")
        |                 ← retour PlanningPage  ← ❌ PAS vers la visite créée
        |                 (l'utilisateur doit aller manuellement dans VisiteList
        |                  pour retrouver la visite et soumettre un rapport)
        |
        └── tap "+ Ajouter une visite" (bouton sticky)
              └── //visits/detail?prefillDate=AUJOURD'HUI
                    [même flux ci-dessus]

AppShell Flyout
  └── //visits
        [VisitListPage.OnAppearing → LoadVisitesCommand]
        |
        └── tap sur une carte visite
              └── //visits/detail?visiteId={id}
                    [VisitDetailPage.OnNavigatedTo → InitAsync → charge visite existante]
                    |
                    ├── si IsCompleted=false :
                    |     ├── tap "Soumettre un rapport" (visible si IsExisting)
                    |     |     └── //visits/rapport?visiteId={id}
                    |     |           [RapportPage.OnAppearing → LoadProduits + PreCapture]
                    |     |           (form: résultat, contenu, produits discutés)
                    |     |           |
                    |     |           └── tap "Soumettre le rapport"
                    |     |                 → GPS capture (10s)
                    |     |                 → POST /rapports/createUpdate
                    |     |                 → GoToAsync("..")
                    |     |                 ← retour VisiteDetailPage
                    |     |                   ⚠️ IsCompleted encore false (pas mis à jour)
                    |     |                   ⚠️ bouton "Soumettre rapport" encore visible
                    |     |
                    |     └── tap "Enregistrer" → GoToAsync("..") ← retour VisiteList
                    |
                    └── si IsCompleted=true :
                          banner "Visite terminée" visible
                          formulaire désactivé
                          ⚠️ bouton "Soumettre rapport" encore visible (bug E1)
```

**Gaps identifiés :**

| Gap | Description |
|-----|-------------|
| G1 | Après création depuis PlanningPage → retour PlanningPage, pas vers la visite créée |
| G2 | Pas de navigation directe PlanningPage → VisiteDetail d'une entrée planning existante |
| G3 | Après soumission rapport → VisiteDetailPage avec IsCompleted toujours false |
| G4 | Bouton "Soumettre rapport" visible après soumission (IsCompleted pas rechargé) |
| G5 | Liaison PlanningEntry → VisiteCreée → Rapport : jamais complète (idPlanning perdu) |

---

## PARTIE 7 — GPS BEST PRACTICE ANALYSIS

### Q1 : Quand demander la permission GPS ?

**Code actuel :** Permission demandée dans `CaptureLocationAsync()` au moment du tap "Soumettre".

**Recommandation :** Demander à l'**ouverture de RapportPage** (`OnAppearing`) :
```csharp
// Dans PreCaptureLocationAsync
var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
if (status != PermissionStatus.Granted) {
    GeoStatus = "⚠️ Permission refusée — coordonnées non disponibles";
    return;  // avertit l'utilisateur tôt
}
var last = await Geolocation.GetLastKnownLocationAsync();
```

Cela évite que le dialogue de permission apparaisse **après** que l'utilisateur a rempli tout le formulaire.

---

### Q2 : Quand avertir l'utilisateur d'activer le GPS ?

**Code actuel :** Aucun avertissement préalable — l'utilisateur découvre l'absence de GPS seulement au moment de la soumission.

**Recommandation :** À l'ouverture de RapportPage, vérifier si le GPS est activé et afficher une bannière d'avertissement visible dès le début :
```
⚠️ GPS désactivé — activez la localisation dans les paramètres
```

Cela permet au délégué d'activer le GPS **pendant qu'il remplit le rapport**, pas juste avant l'envoi.

---

### Q3 : Quel est le meilleur moment pour capturer le GPS ?

**Option C — Les deux (recommandée) :**
- **Sur apparition :** `GetLastKnownLocationAsync()` = position rapide, sans attente, pour confirmer à l'utilisateur que la géolocalisation est active
- **Sur soumission :** `GetLocationAsync(accuracy=Medium, timeout=10s)` = position précise au moment de l'envoi

**Code actuel :** Implémente déjà l'Option C ✅ — mais la permission n'est demandée que lors de la soumission (problème Q1).

---

### Q4 : Que faire si le GPS est indisponible à la soumission ?

**Code actuel :** Option B — soumission avec `null` pour Latitude/Longitude.
```csharp
// CaptureLocationAsync retourne (null, null) sur tout échec
// SubmitAsync continue quand même
rapport.Latitude  = lat;   // peut être null
rapport.Longitude = lon;   // peut être null
```

**Backend :** Accepte `null` (`Latitude` et `Longitude` sont `double?`).

**Recommandation :** L'Option B (soumission avec null) est correcte pour un CRM pharmaceutique. Bloquer la soumission sur absence GPS serait trop restrictif en terrain (zones blanches, bâtiments). Une alternative intermédiaire : afficher un warning "⚠️ GPS non disponible — la visite sera enregistrée sans coordonnées" avant de permettre la soumission.

---

## PARTIE 8 — SUMMARY OF ALL ISSUES FOUND

| # | Étape | Fichier | Issue | Impact | Priorité |
|---|-------|---------|-------|--------|----------|
| 1 | Planning | `PlanningViewModel.cs` | `WeekLabel` affiche `AddDays(6)` (dimanche) mais boucle `i<6` = samedi → label trompeur | Faible | Basse |
| 2 | Planning | `PlanningViewModel.cs` | API fetche 7 jours (endDate=AddDays(6)) mais UI n'affiche que 6 → données dimanche gaspillées | Faible | Basse |
| 3 | Planning | `PlanningViewModel.cs` | `AddVisitAsync` ne passe PAS `idPlanning` → liaison planning-visite perdue | **Élevé** | **Haute** |
| 4 | Planning | `PlanningPage.xaml` | Bouton sticky "+ Ajouter" ne passe pas `idPlanning` non plus | **Élevé** | **Haute** |
| 5 | Planning | `Planning.cs` | `ClientNom` sans `[JsonPropertyName]` → jamais rempli depuis l'API de liste → cartes avec nom vide | Moyen | Moyenne |
| 6 | Planning | `PlanningPage.cs` | Double-load potentiel : `OnAppearing` + `OnWeekStartChanged` | Faible | Basse |
| 7 | Visite création | `VisitDetailViewModel.cs` | `GetUsersByRoleAsync("CLIENT")` pour pharmaciens → mauvais rôle | **Élevé** | **Haute** |
| 8 | Visite création | `VisitDetailViewModel.cs` | `int.Parse(userId ?? "0")` → IdDelegue=0 si session expirée, sans erreur | **Élevé** | **Haute** |
| 9 | Visite création | `VisitDetailViewModel.cs` | Après save → `GoToAsync("..")` → retour planning/liste, pas vers la visite créée | Moyen | Moyenne |
| 10 | Visite création | `VisitDetailViewModel.cs` | Réponse API `Visite?` ignorée après création → VisiteId non récupéré | Moyen | Moyenne |
| 11 | Visite détail | `VisitDetailPage.xaml` | Bouton "Soumettre rapport" `IsVisible="{Binding IsExisting}"` → visible même si `IsCompleted=true` | Moyen | Moyenne |
| 12 | Visite détail | `VisitDetailViewModel.cs` | `_statut` + `StatutOptions` inutiles (Picker Statut supprimé du XAML) — code mort | Faible | Basse |
| 13 | Visite liste | `VisitListViewModel.cs` | Recherche texte uniquement sur TypeLabel, pas sur le nom du contact | Moyen | Moyenne |
| 14 | Visite liste | `VisitListViewModel.cs` | `GetUsersByRoleAsync("CLIENT")` pour pharmaciens → noms jamais résolus (même bug #7) | **Élevé** | **Haute** |
| 15 | Rapport | `RapportPage.xaml.cs` | Permission GPS demandée au moment du "Soumettre" → bloque 10s en fin de formulaire | Moyen | Moyenne |
| 16 | Rapport | `RapportPage.xaml.cs` | Pas d'avertissement "Activez le GPS" à l'ouverture de la page | Faible | Basse |
| 17 | Rapport | `SyncService.cs` | `ProduitsDiscutes` non inclus lors du flush offline → données produits perdues | **Élevé** | **Haute** |
| 18 | Rapport | `SyncService.cs` | `IdDelegue` non défini dans le rapport reconstruit → fallback silencieux SecureStorage | Moyen | Moyenne |
| 19 | Rapport | `RapportViewModel.cs` | Double chargement produits : `OnLinkedVisiteIdChanged` + `OnAppearing` | Faible | Basse |
| 20 | Rapport | `RapportViewModel.cs` | `_resultat` (ancienne prop) + `_selectedResultat` (nouvelle) coexistent — code mort | Faible | Basse |
| 21 | Rapport | `RapportViewModel.cs` | Après soumission, `IsCompleted` non mis à jour localement sur VisiteDetailPage | Moyen | Moyenne |
| 22 | Navigation | `VisitDetailViewModel.cs` | Pas de flux direct PlanningPage → visite créée → soumission rapport | **Élevé** | **Haute** |

---

*Document généré le 2026-05-28 — branche `dev/Mobile-0001`*
