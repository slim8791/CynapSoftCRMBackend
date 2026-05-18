# 🔴 APP CRASH - SOLUTION RAPIDE

## 3 Étapes pour Réparer

### 1️⃣ Trouver Votre Adresse IP (30 secondes)

**Ouvrez PowerShell et exécutez:**
```powershell
ipconfig
```

**Cherchez cette section:**
```
Carte réseau sans fil Wi-Fi :
   Adresse IPv4. . . . . . . . . . : 192.168.1.45
```

**Copez l'adresse IP:** `192.168.1.45` (votre numéro peut être différent)

### 2️⃣ Mettre à Jour le Code (2 minutes)

**Fichier:** `Cynapharm-Mobile/MauiProgram.cs`

**Ligne ~130, remplacez:**
```csharp
❌ AVANT:
var baseUrl = "https://192.168.1.45:7777/";  // avec IP exemple

✅ APRÈS:
var baseUrl = "https://192.168.1.45:7777/";  // avec VOTRE IP réelle
```

**⚠️ IMPORTANT:** Ne gardez pas "192.168.1.X" avec le X littéral!

### 3️⃣ Redéployer (5 minutes)

1. **Nettoyez le build:**
   ```
   Visual Studio → Build → Clean Solution
   ```

2. **Reconstituez:**
   ```
   Visual Studio → Build → Rebuild Solution
   ```

3. **Redéployez:**
   - Connectez Samsung A55 via USB
   - Sélectionnez l'appareil dans le dropdown
   - Appuyez sur **F5**

---

## ✅ Test de Vérification

Après redéploiement, vous devriez voir:

✅ L'app se lance sans crash  
✅ L'écran de login apparaît  
✅ Vous pouvez taper email/password  

---

## 🐛 Si Ça Marche Pas

**Vérifiez dans cet ordre:**

1. ✅ L'adresse IP dans le code = votre vrai IP (pas "192.168.1.X")
2. ✅ L'API Gateway tourne: `https://localhost:7777`
3. ✅ Samsung A55 connecté en USB
4. ✅ USB Debugging activé
5. ✅ Solution rebuildée (pas juste rebuild, mais CLEAN + REBUILD)

---

## 📞 Message d'Erreur = Indice

| Erreur | Cause | Solution |
|--------|-------|----------|
| **Crash immédiat** | IP non configurée | Remplacez "192.168.1.X" |
| **"Connection timeout"** | Mauvaise IP | Vérifiez ipconfig |
| **"Connection refused"** | API non lancé | Démarrez Gateway |
| **Socket error** | Firewall | Ouvrez port 7777 |
| **"Invalid credentials"** | API répond, wrong login | Vérifiez email/password |

---

## ⚡ QuickStart PowerShell

```powershell
# 1. Allez au dossier du projet
cd C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend

# 2. Trouvez votre IP
ipconfig | findstr "IPv4"

# 3. Lancez l'API
cd CynapCRM.Gateway
dotnet run --launch-profile https

# 4. Dans une nouvelle PowerShell
cd C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend\Cynapharm-Mobile

# 5. Nettoyez et rebuilder
dotnet clean -f net10.0-android
dotnet build -f net10.0-android -c Debug
```

---

## 🎯 Résumé

**Cause 95%:** L'IP n'était pas configurée (encore "192.168.1.X")  
**Solution:** Remplacez par votre adresse IP réelle  
**Temps:** 5 minutes max  

🚀 **C'est tout!**

