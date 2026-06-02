# MAUI Test Report — 2026-05-15

## Summary

| Status  | Count |
|---------|-------|
| Passed  | 72    |
| Failed  | 0     |
| Missing | 0     |

**Total duration:** 1.87 s  
**Test runner:** xUnit 2.9.3 · `dotnet test --framework net10.0-windows10.0.19041.0`  
**Test project:** `Cynapharm-Mobile.Tests/`

---

## Models

### CartLine
- [PASS] `HasPromo_ReturnsFalse_WhenRemisePercentageIsZero`
- [PASS] `HasPromo_ReturnsTrue_WhenRemisePercentageIsPositive`
- [PASS] `SousTotal_IsQuantiteTimesEffectiveUnitPrice`
- [PASS] `SousTotal_IsZero_WhenQuantiteIsZero`
- [PASS] `EconomieTotale_IsQuantiteTimesDiscountAmount_WhenPromoActive`
- [PASS] `EconomieTotale_IsZero_WhenNoPriceReduction`

### LigneCommande
- [PASS] `DisplayName_ReturnsProductNom_WhenNonEmpty`
- [PASS] `DisplayName_ReturnsFallbackWithId_WhenProductNomIsEmpty`
- [PASS] `SousTotal_IsQuantiteTimesUnitPrice`

### Order
- [PASS] `NumeroCommande_PadsIdToFiveDigits`
- [PASS] `NumeroCommande_HandlesExactlyFiveDigitId`
- [PASS] `NumeroCommande_HandlesLargeIdWithoutTruncation`

### Objectif
- [PASS] `TypeObjectif_ReturnsExpectedLabel_ForKnownCodes` (× 4 — Visites, Chiffre d'affaires, Nouveaux clients, Fidélisation)
- [PASS] `TypeObjectif_ReturnsGenericLabel_ForUnknownPositiveCode`
- [PASS] `TypeObjectif_ReturnsEmpty_WhenCodeIsZero`
- [PASS] `Periode_ReturnsExpectedLabel_ForKnownCodes` (× 3 — Mensuel, Trimestriel, Annuel)
- [PASS] `Periode_ReturnsGenericLabel_ForUnknownPositiveCode`
- [PASS] `Periode_ReturnsEmpty_WhenCodeIsZero`
- [PASS] `ProgressValue_IsZero_WhenValeurCibleIsZero`
- [PASS] `ProgressValue_IsZero_WhenValeurActuelleIsNull`
- [PASS] `ProgressValue_IsCorrectRatio_WhenPartiallyAchieved`
- [PASS] `ProgressValue_IsClampedAt1_WhenValeurActuelleExceedsTarget`

### PagedResult\<T\>
- [PASS] `HasMore_ReturnsTrue_WhenMorePagesExist`
- [PASS] `HasMore_ReturnsFalse_WhenOnLastPage`
- [PASS] `HasMore_ReturnsFalse_WhenPageTimeSizeExactlyEqualsTotalCount`
- [PASS] `HasMore_ReturnsFalse_WhenTotalCountIsZero`

### ApiResponse\<T\>
- [PASS] `IsSuccess_IsTrueByDefault`
- [PASS] `Success_IsAliasForIsSuccess`
- [PASS] `Data_IsAliasForResult`
- [PASS] `Success_ReflectsIsSuccess_AfterChange`

### StockDisplayItem
- [PASS] `HasExpiry_ReturnsTrue_WhenExpiryLabelIsNotNull`
- [PASS] `HasExpiry_ReturnsFalse_WhenExpiryLabelIsNull`
- [PASS] `CanDistribute_ReturnsTrue_WhenIsEchantillonAndStockPositive`
- [PASS] `CanDistribute_ReturnsFalse_WhenNotEchantillon`
- [PASS] `CanDistribute_ReturnsFalse_WhenEchantillonButZeroStock`
- [PASS] `CanDistribute_ReturnsFalse_WhenEchantillonButNegativeStock`

---

## Services

### MemoryCacheService
- [PASS] `GetOrCreateAsync_InvokesFactory_OnCacheMiss`
- [PASS] `GetOrCreateAsync_ReturnsCachedValue_WithoutCallingFactory_OnCacheHit`
- [PASS] `GetOrCreateAsync_InvokesFactory_AfterTtlExpires`
- [PASS] `GetOrCreateAsync_DoesNotCacheNullResult`
- [PASS] `Invalidate_RemovesEntry_SoNextCallInvokesFactory`
- [PASS] `InvalidateAll_ClearsAllEntries_SoSubsequentCallsInvokeFactory`
- [PASS] `GetOrCreateAsync_ReturnsDifferentValues_ForDifferentKeys`

### TaskExtensions
- [PASS] `SafeFireAndForget_CompletesWithoutException_WhenTaskSucceeds`
- [PASS] `SafeFireAndForget_InvokesOnError_WhenTaskThrows`
- [PASS] `SafeFireAndForget_DoesNotThrow_WhenOnErrorCallbackIsNull`

---

## ViewModels

### RapportViewModel
- [PASS] `Title_IsSetOnConstruction`
- [PASS] `IsBusy_IsFalseOnConstruction`
- [PASS] `ResultatOptions_ContainsThreeExpectedValues`
- [PASS] `CanSubmit_IsTrueInitially_BeforeValidationRuns`
- [PASS] `ContenuError_ShowsRequiredMessage_WhenContenuSetToEmpty`
- [PASS] `CanSubmit_IsFalse_WhenContenuFailsValidation`
- [PASS] `ContenuError_ShowsMinLengthMessage_WhenContenuIsTooShort`
- [PASS] `ContenuError_IsEmpty_AndCanSubmit_WhenContenuMeetsMinLength`

### CreateOrderViewModel
- [PASS] `CartTotal_IsZero_WhenCartLinesIsEmpty`
- [PASS] `CartTotal_SumsSousTotal_AcrossAllLines`
- [PASS] `CartSavings_IsZero_WhenNoPromoApplied`
- [PASS] `CartSavings_SumsEconomieTotale_AcrossPromoLines`
- [PASS] `HasCartSavings_IsFalse_WhenCartSavingsIsZero`
- [PASS] `HasCartSavings_IsTrue_WhenCartSavingsIsPositive`
- [PASS] `IsStep1_IsTrue_OnInitialStep`
- [PASS] `NextStep_SetsErrorMessage_WhenCartIsEmptyOnStep1`
- [PASS] `NextStep_AdvancesToStep2_WhenCartHasLines`
- [PASS] `PreviousStep_DoesNothing_WhenAlreadyOnStep1`
- [PASS] `PreviousStep_DecrementsStep_WhenBeyondStep1`
- [PASS] `RemoveLine_RemovesLineFromCart`
- [PASS] `RemoveLine_DoesNothing_WhenLineIsNull`

---

## Out-of-scope Classes (untested — no public logic beyond DI storage)

The following classes were analysed and excluded from the test suite because they are either pure DTOs (no computed properties), abstract base infrastructure, or platform-specific entry points that cannot execute in a headless runner without full WinRT host:

| Class | Reason |
|-------|--------|
| `LoginRequest`, `ChangePasswordRequest`, `ForgotPasswordRequest`, `LoginResponse`, `UserInfo` | Plain DTOs, no computed properties or validation attributes |
| `Facture`, `BonCommande`, `BonLivraison`, `DocumentSummary` | Plain DTOs |
| `Visite`, `Rapport`, `Planning`, `Kpi`, `Region` | Plain DTOs |
| `StockDelegue`, `StockMouvement`, `StockPromo` | Plain DTOs |
| `Product`, `Lot`, `Promotion`, `ProductCheckItem`, `LogEntry` | Plain DTOs |
| `ApiService`, `AuthService`, `OrderService`, `ProductService`, `VisiteService`, `DocumentService`, `InventoryService`, `KpiService`, `PlanningService` | Network services — testing requires mocking `HttpClient` and verifying API contracts; recommended as a separate integration test suite with `MockHttpMessageHandler` |
| `LocalDatabaseService` | SQLite service — `FileSystem.AppDataDirectory` requires a WinRT COM host; test via a real device or integration test profile |
| `SyncService` | Depends on `LocalDatabaseService` (same reason) |
| `ShellNavigationService` | Depends on `Shell.Current` — requires MAUI host |
| `HapticService`, `AppLogger`, `CrashLogger` | Platform-specific or logging infrastructure |
| `LoginViewModel`, `ForgotPasswordViewModel`, `DashboardViewModel`, `DocumentListViewModel`, `DocumentDetailViewModel`, `ObjectifViewModel`, `OrderDetailViewModel`, `OrderListViewModel`, `PlanningViewModel`, `ProductDetailViewModel`, `ProductListViewModel`, `ProfileViewModel`, `MyStockViewModel`, `VisitDetailViewModel`, `VisitListViewModel` | All relay on `Shell.Current` or `LocalDatabaseService` in their relay commands; constructor-level state and pure properties not covered here are candidates for a future device test suite |

> **Note on ViewModel testing strategy**: `RapportViewModel` and `CreateOrderViewModel` were testable by passing `null` for `LocalDatabaseService` because neither constructor calls `localDb` directly. The tests exercise only synchronous, pure-computation code paths. Relay commands that use `localDb`, `Shell.Current`, or `Connectivity` were deliberately not invoked.

---

## Verdict

**PASS** — 72 / 72 tests passed, 0 failures.
