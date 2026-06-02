# Plan d'Amélioration - Application MAUI Cynapharm Mobile

**Date:** Mai 14, 2026  
**Application:** Cynapharm-Mobile (MAUI)  
**Version actuelle:** Phase initiale  

---

## 📋 Résumé Exécutif

L'application MAUI Cynapharm est en phase de développement actif. Ce plan identifie **17 domaines critiques d'amélioration** couvrant l'architecture, la stabilité, la sécurité, et l'expérience utilisateur. Les priorités sont classées par **impact et urgence**.

---

## 🔴 **PHASE 1: STABILITÉ CRITIQUE (Semaine 1-2)**

### 1.1 Résoudre les Crash Android Runtime
**Problème:** `Android.Runtime.JavaProxyThrowable` lors de la navigation après login  
**Impact:** 🔴 CRITIQUE - Application inutilisable  
**Symptômes:**
- Login échoue avec erreur générique non déboguée
- Stack trace indéchiffrable dans le debugger
- Crash survient à la transition `//products`

**Solutions:**
- [ ] Implémenter exception handling au niveau Activity/Fragment
- [ ] Créer un `PlatformExceptionHandler` pour intercepter les erreurs Android
- [ ] Ajouter logging détaillé via `logcat` au lieu du debugger VS
- [ ] Tester navigation avec **MainThread dispatch** sur le thread principal
- [ ] Vérifier les permissions Android pour SecureStorage dans `AndroidManifest.xml`

**Fichiers à modifier:**
```
Platforms/Android/MainActivity.cs → Ajouter try-catch global
Services/ApiService.cs → Améliorer exception handling
MauiProgram.cs → Global exception handler renforcé
```

---

### 1.2 Configurer Correctement le Routage Shell
**Problème:** Le dashboard n'est pas accessible pour l'utilisateur ADMIN  
**Impact:** 🔴 CRITIQUE - Certains rôles ne peuvent pas naviguer  

**Solutions:**
- [ ] Fixer `ApplyRoleVisibility()` pour inclure ADMIN dans les pages accessibles
- [ ] Implémenter delayed navigation avec vérification de route availability
- [ ] Ajouter validation de route avant `GoToAsync()`
- [ ] Tester tous les chemins de navigation pour chaque rôle

**Fichiers à modifier:**
```
AppShell.xaml.cs → Ajouter rôle ADMIN aux visibilités appropriées
AppShell.xaml → Vérifier toutes les routes ShellContent
ViewModels/Auth/LoginViewModel.cs → Rôle-basé navigation robuste
```

---

### 1.3 Améliorer la Gestion des Erreurs HTTP
**Problème:** Erreurs API vagues, mauvaise distinction entre erreurs réseau/authentification  
**Impact:** 🟠 HAUT - Expérience utilisateur confuse

**Solutions:**
- [ ] Créer classe `HttpErrorHandler` pour mapper les codes HTTP à messages lisibles
- [ ] Distinguer: erreur réseau vs erreur API vs non-autorisation
- [ ] Implémenter retry automatique pour erreurs temporaires (500, 503)
- [ ] Ajouter logging structuré en JSON pour debugging

**Code à créer:**
```csharp
// Services/HttpErrorHandler.cs
public static class HttpErrorHandler
{
    public static string GetUserFriendlyMessage(HttpStatusCode code, string serverMessage = null)
    {
        return code switch
        {
            HttpStatusCode.Unauthorized => "Session expirée. Veuillez vous reconnecter.",
            HttpStatusCode.Forbidden => "Vous n'avez pas les permissions nécessaires.",
            HttpStatusCode.NotFound => "La ressource n'existe plus.",
            HttpStatusCode.BadGateway => "Le serveur est temporairement indisponible. Réessayez dans quelques secondes.",
            _ => serverMessage ?? "Une erreur s'est produite. Veuillez réessayer."
        };
    }
}
```

---

## 🟠 **PHASE 2: ARCHITECTURE & SÉCURITÉ (Semaine 3-4)**

### 2.1 Implémenter Token Refresh Automatique
**Problème:** Token JWT dure 15-30 min, utilisateur logout sans raison  
**Impact:** 🟠 HAUT - Mauvaise UX sur longues sessions

**Solutions:**
- [ ] Créer middleware `TokenRefreshHandler` dans `ApiService`
- [ ] Détecter token expiration avant chaque request
- [ ] Implémenter refresh token silencieux
- [ ] Rediriger vers login si refresh échoue

**Architecture:**
```
ApiService.cs
├── PrepareAuthHeaderAsync() [AMÉLIORER]
│   ├── Vérifier expiration JWT
│   ├── Si < 2 min avant expiration → RefreshToken()
│   └── Si expiré → Rediriger login
└── HandleResponseAsync() [GARDER]
```

---

### 2.2 Sécuriser le Stockage des Credentials
**Problème:** Token stocké en SecureStorage, mais pas de protection contre jailbreak  
**Impact:** 🟠 HAUT - Risque de sécurité

**Solutions:**
- [ ] Utiliser Keychain (iOS) et KeyStore (Android) via SecureStorage
- [ ] Implémenter Biometric Auth optionnel (Face/Fingerprint)
- [ ] Ajouter Certificate Pinning pour connexion au gateway
- [ ] Implémenter Device ID pour sessions liées au device

**Fichiers à créer:**
```
Services/SecurityService.cs
├── IsBiometricAvailable()
├── AuthenticateWithBiometric()
├── PinCertificate()
└── GetDeviceId()
```

---

### 2.3 Ajouter Logging & Monitoring
**Problème:** Impossible de déboguer erreurs en production  
**Impact:** 🟠 HAUT - Support technique impossible

**Solutions:**
- [ ] Implémenter `ILogger` via Microsoft.Extensions.Logging
- [ ] Configurer log file local sur device (DEBUG mode)
- [ ] Envoyer logs critiques au serveur backend
- [ ] Structurer logs en JSON pour parsing facile

**Services à créer:**
```
Services/LoggingService.cs
├── LogInfo(message, context)
├── LogError(exception, context)
├── LogPerformance(operationName, duration)
└── SendLogsToBackend()

Data/ (local storage)
└── logs/
    ├── app-2026-05-14.log
    └── errors-2026-05-14.log
```

---

## 🟡 **PHASE 3: CODE QUALITY & ORGANIZATION (Semaine 5-6)**

### 3.1 Refactoriser le Modèle MVVM
**Problème:** ViewModels font trop de choses (business logic + API + navigation)  
**Impact:** 🟡 MOYEN - Difficile à tester, code dupliqué

**Solutions:**
- [ ] Créer layer `UseCases` pour business logic
- [ ] Implémenter `Repository` pattern pour isolation des données
- [ ] Séparer concern: ViewModel = UI state, UseCase = business logic
- [ ] Ajouter Unit Tests pour ViewModels

**Nouvelle structure:**
```
ViewModels/
├── Auth/
│   └── LoginViewModel.cs [ALLÉGÉ]
UseCases/
├── Auth/
│   ├── ILoginUseCase.cs [NOUVEAU]
│   └── LoginUseCase.cs [NOUVEAU]
Repositories/
├── IAuthRepository.cs [NOUVEAU]
├── AuthRepository.cs [NOUVEAU]
```

---

### 3.2 Implémenter Dependency Injection Correctement
**Problème:** Services créés manuellement, difficult à tester  
**Impact:** 🟡 MOYEN - Testing impossible

**Solutions:**
- [ ] Utiliser interfaces pour tous les services
- [ ] Configurer DI dans `MauiProgram.cs` avec registrations claires
- [ ] Créer `ServiceCollectionExtensions` pour organiser registrations
- [ ] Implémenter `Singleton`, `Transient`, `Scoped` appropriément

**Code:**
```csharp
// Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Data
    services.AddScoped<IAuthRepository, AuthRepository>();
    services.AddScoped<IProductRepository, ProductRepository>();
    
    // UseCases
    services.AddScoped<ILoginUseCase, LoginUseCase>();
    
    // Services
    services.AddScoped<AuthService>();
    services.AddScoped<ApiService>();
    
    return services;
}

// MauiProgram.cs
builder.Services.AddApplicationServices();
```

---

### 3.3 Créer Base Repositories & Models
**Problème:** Pas de structure unifiée pour data access  
**Impact:** 🟡 MOYEN - Duplication de code

**Solutions:**
- [ ] Créer `IRepository<T>` générique
- [ ] Implémenter `BaseRepository<T>` avec CRUD commun
- [ ] Typer correctement tous les models API
- [ ] Valider models à la réception

**Structure:**
```
Repositories/
├── IRepository.cs [GENERIC]
├── BaseRepository.cs [GENERIC]
├── Auth/
│   └── IAuthRepository.cs [SPÉCIFIQUE]
└── Products/
    └── IProductRepository.cs [SPÉCIFIQUE]

Models/
├── Dtos/
│   ├── LoginRequestDto.cs
│   ├── LoginResponseDto.cs
│   └── ... [TYPAGE STRICT]
├── Domain/
│   ├── User.cs
│   └── Product.cs
```

---

## 🟡 **PHASE 4: FEATURES & EXPÉRIENCE (Semaine 7-8)**

### 4.1 Ajouter Offline Support
**Problème:** Application inutilisable sans internet  
**Impact:** 🟡 MOYEN - Usage réel limité

**Solutions:**
- [ ] Implémenter cache local SQLite
- [ ] Sync des données une fois connecté
- [ ] Afficher "mode offline" dans l'UI
- [ ] Queue des opérations offline

**Architecture:**
```
Services/
├── OfflineSyncService.cs [NOUVEAU]
└── LocalCacheService.cs [NOUVEAU]

Data/
└── LocalCache.db [NOUVELLE]
```

---

### 4.2 Implémenter Pull-to-Refresh & Pagination
**Problème:** Listes chargent tout à la fois, pas de rafraîchissement facile  
**Impact:** 🟡 MOYEN - Performance + UX

**Solutions:**
- [ ] Ajouter `RefreshView` aux pages listes
- [ ] Implémenter pagination (limit/offset)
- [ ] Ajouter infinite scroll
- [ ] Montrer skeleton loading

**Exemple:**
```xml
<!-- Views/Visites/VisitListPage.xaml [À AJOUTER] -->
<RefreshView Command="{Binding RefreshCommand}" IsRefreshing="{Binding IsBusy}">
    <CollectionView ItemsSource="{Binding Visites}" 
                    SelectionChangedCommand="{Binding SelectVisiteCommand}">
        <CollectionView.ItemTemplate>
            <DataTemplate>
                <VisitCard />
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</RefreshView>
```

---

### 4.3 Ajouter Notifications Push & Local
**Problème:** Pas de notifications, utilisateur ne sait pas des updates  
**Impact:** 🟡 MOYEN - Engagement faible

**Solutions:**
- [ ] Implémenter Firebase Cloud Messaging (FCM)
- [ ] Ajouter local notifications pour reminders
- [ ] Badge count sur app icon
- [ ] Deep linking depuis notifications

**Services à créer:**
```
Services/
├── PushNotificationService.cs [NOUVEAU]
└── LocalNotificationService.cs [NOUVEAU]
```

---

## 🟢 **PHASE 5: TESTING & QUALITÉ (Semaine 9-10)**

### 5.1 Ajouter Unit Tests
**Problème:** Pas de tests, risque de régression haut  
**Impact:** 🟢 FAIBLE URGENCE - Important pour qualité

**Solutions:**
- [ ] Créer projets `Cynapharm-Mobile.Tests`
- [ ] Tests ViewModels avec Mock services
- [ ] Tests Services avec Mock HttpClient
- [ ] Couvrir cas d'erreur

**Structure:**
```
Cynapharm-Mobile.Tests/
├── ViewModels/
│   ├── LoginViewModelTests.cs
│   └── DashboardViewModelTests.cs
├── Services/
│   ├── AuthServiceTests.cs
│   └── ApiServiceTests.cs
└── UseCases/
    └── LoginUseCaseTests.cs
```

---

### 5.2 Ajouter UI Tests
**Problème:** Pas de tests d'intégration UI  
**Impact:** 🟢 FAIBLE URGENCE - Important pour stabilité

**Solutions:**
- [ ] Utiliser Appium ou MAUI Testing Framework
- [ ] Tests workflow critique: Login → Navigation → Logout
- [ ] Tests role-based visibility

---

## 🟢 **PHASE 6: PERFORMANCE & DEPLOYMENT (Semaine 11-12)**

### 6.1 Optimiser Performance
**Problème:** Possibles lags, pas de profiling fait  
**Impact:** 🟢 FAIBLE URGENCE

**Solutions:**
- [ ] Profiler avec Xamarin Profiler
- [ ] Implémenter virtualization (RecycleView)
- [ ] Lazy load pour images
- [ ] Async/await optimizations

---

### 6.2 Configurer Build Release & Signing
**Problème:** Pas de processus de build formalisé  
**Impact:** 🟢 FAIBLE URGENCE - Important pour production

**Solutions:**
- [ ] Créer scripts build pour Android/iOS
- [ ] Configurer Keystore Android
- [ ] Configurer Certificate iOS
- [ ] Automatiser via CI/CD (GitHub Actions)

**Fichiers à créer:**
```
build/
├── android-release.sh
├── ios-release.sh
└── .github/workflows/
    ├── android-build.yml
    └── ios-build.yml
```

---

## 📊 Tableau Priorités

| Phase | Domaine | Priorité | Délai | Effort | Impact |
|-------|---------|----------|-------|--------|--------|
| 1 | Crash Android | 🔴 CRITIQUE | Urgent | 1-2j | Bloquant |
| 1 | Routage Shell | 🔴 CRITIQUE | Urgent | 1-2j | Bloquant |
| 1 | Erreurs HTTP | 🟠 HAUT | 1 sem | 2-3j | Expérience |
| 2 | Token Refresh | 🟠 HAUT | 1 sem | 3-4j | Stabilité |
| 2 | Sécurité/Biometric | 🟠 HAUT | 2 sem | 4-5j | Sécurité |
| 2 | Logging | 🟠 HAUT | 2 sem | 3-4j | Debugging |
| 3 | Refactor MVVM | 🟡 MOYEN | 3 sem | 5-7j | Maintenance |
| 3 | DI Proper | 🟡 MOYEN | 3 sem | 3-4j | Testabilité |
| 4 | Offline Support | 🟡 MOYEN | 4 sem | 7-10j | Features |
| 5 | Unit Tests | 🟢 FAIBLE | 5 sem | 8-10j | Qualité |
| 6 | Performance | 🟢 FAIBLE | 6 sem | 5-7j | UX |

---

## 🔧 Quick Wins (Fait en 1-2 jours)

- [ ] Fixer crash Android avec proper exception handling
- [ ] Ajouter ADMIN au visibility logic
- [ ] Améliorer messages d'erreur HTTP
- [ ] Ajouter logging structuré
- [ ] Documenter API endpoints et rate limits

---

## 📝 Checklist de Prochaines Actions

### Cette semaine (URGENT):
- [ ] Déboguer crash Android avec logcat
- [ ] Fixer routage Shell pour ADMIN
- [ ] Tester tous les chemins de navigation
- [ ] Documenter l'architecture actuelle
- [ ] Créer GitHub Issues pour chaque tâche

### Prochaine sprint:
- [ ] Implémenter token refresh
- [ ] Ajouter Certificate Pinning
- [ ] Créer structure repositories
- [ ] Planifier sessions de pair programming

### Architecture & Design Decisions:
- [ ] Documentation d'architecture (ADR - Architecture Decision Records)
- [ ] Définir conventions de code (style guide)
- [ ] Setup linting/formatting (StyleCop, EditorConfig)
- [ ] Planning sessions avec équipe backend

---

## 📚 Ressources Recommandées

- **MAUI Best Practices:** https://learn.microsoft.com/en-us/dotnet/maui/
- **MVVM Community Toolkit:** https://github.com/CommunityToolkit/MVVM-Samples
- **Exception Handling:** https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/exception-handling
- **Secure Storage:** https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/secure-storage
- **Testing in MAUI:** https://github.com/microsoft/maui-samples

---

## 📞 Support & Escalation

**Issues Critiques (Blockers):**
- Crash Android → Escalate à équipe plateforme
- Gateway indisponible → Vérifier configuration ocelot.json
- Token expiration → Coordonner avec AuthAPI

**Points de Décision:**
- Biometric Auth: Yes/No? (Dépend des specs client)
- Offline Support: Priorité? (Dépend des use cases)
- Push Notifications: Firebase ou autre service?

---

**Prochaine Revision:** Mai 28, 2026  
**Owner:** Équipe Frontend MAUI  
**Approved by:** -
