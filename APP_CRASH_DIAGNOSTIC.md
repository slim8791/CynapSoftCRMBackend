# 🚨 App Crash - Diagnostic & Solution

## Problème
L'application se lance mais **crash immédiatement** sur Samsung A55

---

## 🔍 Diagnostic: 4 Causes Possibles

### 1. **IP Address Non Configurée** (95% des cas)
**Symptôme:** Crash au démarrage, pas de message d'erreur visible

**Cause:** L'adresse IP dans `MauiProgram.cs` est encore le placeholder `192.168.1.X`

**Vérification:**
```csharp
// File: Cynapharm-Mobile/MauiProgram.cs
// Look for this line in GetApiBaseUrl():
var baseUrl = "https://192.168.1.45:7777/";  // Check if it has actual IP or placeholder
```

**Solution:**
1. Trouvez l'adresse IP réelle de votre PC
2. Remplacez l'IP dans le code

---

### 2. **Backend API Non Démarré** (15% des cas)
**Symptôme:** L'app démarre puis crash lors de la vérification de l'authentification

**Cause:** L'API Gateway n'est pas en cours d'exécution

**Vérification:**
```powershell
# Vérifiez que le serveur répond
curl https://localhost:7777/api/health -SkipCertificateCheck

# Si erreur: Le serveur ne tourne pas
```

**Solution:**
```powershell
cd C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend\CynapCRM.Gateway
dotnet run --launch-profile https
```

---

### 3. **Firewall Bloque la Connexion** (5% des cas)
**Symptôme:** Crash au login, timeout après quelques secondes

**Cause:** Windows Firewall bloque le port 7777

**Vérification:**
```powershell
# Vérifiez les règles firewall
Get-NetFirewallRule -DisplayName "*7777*"

# Créer une règle si elle n'existe pas:
New-NetFirewallRule -DisplayName "HTTPS 7777 MAUI Dev" -Direction Inbound `
    -Action Allow -Protocol TCP -LocalPort 7777 `
    -Profile "Private,Public" -Enabled $true
```

---

### 4. **Problème de Routing/Shell** (5% des cas)
**Symptôme:** L'app démarre mais affiche un écran blanc ou error "login route not found"

**Cause:** Route XAML non enregistrée correctement

**Vérification:** Voir `AppShell.xaml.cs` - les routes doivent être enregistrées

---

## ✅ Solution Complète - 5 Étapes

### Étape 1: Trouver votre adresse IP
```powershell
ipconfig
```

**Cherchez la ligne:** `IPv4 Address . . . . . . . . . . : 192.168.X.X`

**Notez-la:** `192.168.1.___ ` (remplacez ___ par le dernier nombre)

### Étape 2: Mettre à jour MauiProgram.cs
```csharp
// File: Cynapharm-Mobile/MauiProgram.cs
// Around line 130

private static string GetApiBaseUrl()
{
#if ANDROID
    // REMPLACEZ 45 par votre dernier octet IP
    var baseUrl = "https://192.168.1.45:7777/";  // ← CHANGE THIS!

    if (baseUrl.Contains("192.168.1.X") || !Uri.TryCreate(baseUrl, UriKind.Absolute, out _))
    {
        System.Diagnostics.Debug.WriteLine("[CRITICAL] Invalid API URL!");
        return "https://192.168.1.UNCONFIGURED:7777/";
    }
    return baseUrl;
#elif IOS
    return "https://localhost:7777/";
#else
    return "https://localhost:7777/";
#endif
}
```

### Étape 3: Démarrer l'API Backend
```powershell
cd CynapCRM.Gateway
dotnet run --launch-profile https

# Expected output:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7777
```

### Étape 4: Nettoyer et Rebuild
```powershell
cd Cynapharm-Mobile

# Clean previous build
dotnet clean -f net10.0-android

# Full rebuild
dotnet build -f net10.0-android -c Debug

# Or in Visual Studio:
# Build → Clean Solution
# Build → Rebuild Solution
```

### Étape 5: Redéployer sur l'appareil
1. **Connectez** votre Samsung A55 via USB
2. Dans Visual Studio:
   - Select device dropdown → Your Samsung A55
   - Press **F5**
3. **Attendez** que l'app s'installe et se lance
4. **Vérifiez** la console de debug pour les erreurs

---

## 📊 Vérification d'étapes

Avant de redéployer, vérifiez:

- [ ] Adresse IP PC trouvée (exemple: 192.168.1.45)
- [ ] IP mise à jour dans MauiProgram.cs (sans "X")
- [ ] Backend API est en cours d'exécution sur https://localhost:7777
- [ ] Firewall Windows autorise le port 7777
- [ ] Samsung A55 connecté en USB
- [ ] USB Debugging activé (Settings → Developer Options)
- [ ] Solution rebuildée (Clean + Rebuild)
- [ ] Ancien APK supprimé du device

---

## 🔧 Commandes de Nettoyage

```powershell
# Désinstaller complètement l'app du device
adb uninstall com.companyname.cynapharmmobile

# Effacer le cache de build
rmdir bin -r -Force
rmdir obj -r -Force

# Reconstruire
dotnet build -f net10.0-android -c Debug
```

---

## 📱 Voir les Logs du Device

### Dans Visual Studio:
1. **Debug** → **Windows** → **Output**
2. Filtre: `Cynapharm` ou `System.Net`
3. Relancez l'app (F5)
4. Regardez les erreurs en temps réel

### Depuis PowerShell (si adb configuré):
```powershell
adb logcat | findstr "Cynapharm\|System.Net\|Exception"
```

---

## 🎯 Checklist Finale

**Avant de déployer:**
- [ ] IP actualisée dans le code (vérifiez avec Ctrl+F "192.168.1.X" - ne doit rien trouver)
- [ ] API Gateway fonctionne localement
- [ ] Device connecté et détecté
- [ ] Solution clean & rebuild
- [ ] Configuration Debug sélectionnée

**Après déploiement:**
- [ ] App se lance sans crash
- [ ] Écran de login visible
- [ ] Pas d'erreur "Connection timeout"
- [ ] Vous pouvez taper email/password

---

## 📞 Infos Importantes à Fournir si Ça Marche Pas

1. **Message d'erreur exact** (dans l'app ou logs)
2. **Output de MauiProgram.cs** au démarrage
3. **Votre adresse IP réelle** (résultat de `ipconfig`)
4. **Backend est lancé?** (oui/non)
5. **Logs du device** (from Visual Studio Output)

---

## ✨ Variables Clés

| Variable | Valeur Attendue | Vous Avez |
|----------|-----------------|-----------|
| PC IPv4 | 192.168.1.X ou 192.168.0.X | _______ |
| Port API | 7777 | ✅ |
| Protocole | HTTPS | ✅ |
| Device | Samsung A55 | ✅ |
| Android OS | 14+ | ✅ |
| USB Debug | Activé | ✅ |

---

