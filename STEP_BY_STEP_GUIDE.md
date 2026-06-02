# 📱 Samsung A55 - Configuration Étape par Étape

## ⏱️ Temps Total: 5 minutes

---

## 🔴 ÉTAPE 1: Trouver Votre Adresse IP (1 minute)

### Action
1. Ouvrez **PowerShell** (Windows key + "powershell")
2. Tapez:
   ```powershell
   ipconfig
   ```
3. Appuyez sur **Entrée**

### Résultat Attendu
```
Carte réseau sans fil Wi-Fi :
   Adresse IPv4. . . . . . . . . . : 192.168.1.45
   Masque de sous-réseau. . . . . . : 255.255.255.0
   Passerelle par défaut. . . . . . : 192.168.1.1
```

### Votre IP: `192.168.1._____` (notez le nombre à la fin)

---

## 🔴 ÉTAPE 2: Mettre à Jour le Code (2 minutes)

### Fichier à Modifier
**Chemin:** `C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend\Cynapharm-Mobile\MauiProgram.cs`

### Action dans Visual Studio
1. **Ouvrez** le fichier `MauiProgram.cs`
2. Appuyez sur **Ctrl+G** (Go to Line)
3. Tapez: `120`
4. Appuyez sur **Entrée**

### Trouvez Cette Ligne (autour de la ligne 120)
```csharp
var baseUrl = "https://192.168.1.45:7777/";
```

### Remplacez le Nombre
**AVANT:**
```csharp
var baseUrl = "https://192.168.1.45:7777/";  // Exemple - PAS VOTRE IP!
```

**APRÈS:**
```csharp
var baseUrl = "https://192.168.1.XX:7777/";  // XX = votre dernier nombre d'IP
```

### Exemple
Si votre IP est `192.168.1.78`, remplacez par:
```csharp
var baseUrl = "https://192.168.1.78:7777/";
```

### Sauvegarder
**Ctrl+S**

---

## 🔴 ÉTAPE 3: Démarrer l'API Backend (1 minute)

### Action
1. Ouvrez une nouvelle **PowerShell**
2. Exécutez:
   ```powershell
   cd C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend\CynapCRM.Gateway
   dotnet run --launch-profile https
   ```
3. Attendez le message:
   ```
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: https://localhost:7777
   ```

✅ **L'API est maintenant en cours d'exécution!**

---

## 🔴 ÉTAPE 4: Nettoyer & Rebuilder (1 minute)

### Dans Visual Studio

**Option A - Menu:**
1. **Build** → **Clean Solution**
2. Attendez que ce soit terminé
3. **Build** → **Rebuild Solution**
4. Attendez la fin (barre verte)

**Option B - Terminal:**
```powershell
cd Cynapharm-Mobile
dotnet clean -f net10.0-android
dotnet build -f net10.0-android -c Debug
```

✅ **Compilation terminée sans erreurs**

---

## 🔴 ÉTAPE 5: Connecter et Déployer (1 minute)

### Sur Samsung A55

1. **Connectez-le en USB** à votre PC
2. **Confirmez** l'accès (si une fenêtre apparaît)
3. Vérifiez **Settings → Developer Options → USB Debugging** est **ON**

### Dans Visual Studio

1. Regardez le **device dropdown** en haut (actuellement "Windows Machine")
2. **Cliquez** sur le dropdown
3. **Sélectionnez** votre Samsung A55 (ex: "SM-A550F")
4. Appuyez sur **F5** (Start Debugging)

### Attendez
- Compilation: 30 secondes
- Installation: 30 secondes
- Lancement: 10 secondes

✅ **L'app devrait s'ouvrir sur votre téléphone!**

---

## ✅ Vérification Finale

**L'écran que vous devriez voir:**

1. ✅ Logo Cynapharm ou splash screen
2. ✅ Écran d'identification (email + password)
3. ✅ Pas de crash
4. ✅ Pas de message d'erreur rouge

**Test de login:**
1. Entrez vos identifiants de test
2. Appuyez sur "Connexion"
3. Vous devriez voir soit:
   - ✅ Le tableau de bord (si login réussi)
   - ⚠️ Message d'erreur (si login échoué)

**NE DOIT PAS ARRIVER:**
- ❌ Crash de l'app
- ❌ Écran blanc
- ❌ "App a cessé de répondre"

---

## 🐛 Si Quelque Chose Ne Va Pas

### Symptôme: App Crash Immédiat

**Vérifiez:**
1. Votre IP dans le code = adresse réelle (pas "192.168.1.X")
2. Le backend API est lancé
3. La solution a été rebuilée (Clean + Rebuild, pas juste Rebuild)

### Symptôme: "Connection Timeout" au Login

**Vérifiez:**
1. Votre PC IP est correct (faites ipconfig à nouveau)
2. Samsung A55 est sur le MÊME Wi-Fi que votre PC
3. Firewall Windows autorise le port 7777

### Symptôme: "Invalid Credentials"

**C'est normal!** L'API répond, juste mauvais email/password.
Essayez avec un compte de test valide.

---

## 📋 Checklist Avant Déploiement

- [ ] Adresse IP trouvée (ipconfig)
- [ ] IP mise à jour dans MauiProgram.cs (avec VOTRE numéro)
- [ ] API Gateway fonctionne (https://localhost:7777 accessible)
- [ ] Solution cleanée et rebuildée
- [ ] Samsung A55 connecté en USB
- [ ] USB Debugging activé
- [ ] Configuration Debug sélectionnée
- [ ] Device sélectionné dans le dropdown

---

## 🎯 Commandes Rapides

### Si vous voulez faire tout en PowerShell:

```powershell
# 1. Voir votre IP
ipconfig | findstr "IPv4.*192"

# 2. Aller au projet
cd C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend

# 3. Lancer l'API
Start-Process powershell -ArgumentList 'cd CynapCRM.Gateway; dotnet run --launch-profile https'

# 4. Nettoyer et builder
cd Cynapharm-Mobile
dotnet clean -f net10.0-android
dotnet build -f net10.0-android -c Debug

# 5. Redéployer depuis Visual Studio (F5)
```

---

## 🎉 Succès!

Une fois que l'app fonctionne:

1. ✅ Testez le login avec vos identifiants
2. ✅ Vérifiez les données chargent
3. ✅ Testez quelques fonctions
4. ✅ Célébrez! 🎊

---

## 📸 Screenshots de Référence

### PowerShell - ipconfig
```
Carte réseau sans fil Wi-Fi :
   État des médias. . . . . . . . . : Média connecté
   Adresse IPv4. . . . . . . . . . : 192.168.1.45  ← NOTEZ CE NOMBRE
```

### Visual Studio - Device Dropdown
```
Windows Machine
> SM-A550F (Samsung A55) ← Cliquez ici
Emulator
```

### Android Studio - USB Debugging
```
Settings → About Phone → Build Number (tap 7x) → Developer Options
→ USB Debugging [✓] ON
```

---

## ❓ Questions Fréquentes

**Q: Puis-je utiliser le même IP pour chaque build?**
A: Oui, jusqu'à ce que votre adresse IP change. Vérifiez avec ipconfig régulièrement.

**Q: L'API doit-elle toujours tourner?**
A: Oui, pendant que vous testez l'app. Vous pouvez l'arrêter après.

**Q: Puis-je tester sur un émulateur?**
A: Oui, changez l'IP en "10.0.2.2" (spécial pour émulateur).

**Q: Combien de temps prend le premier déploiement?**
A: 3-5 minutes. Les suivants sont plus rapides (1-2 min).

**Q: Que faire si je change de réseau Wi-Fi?**
A: Refaites ipconfig et mettez à jour le code.

---

## 🚀 Vous êtes Prêt!

Suivez les 5 étapes et votre app devrait fonctionner sur Samsung A55!

**Besoin d'aide?** Vérifiez les logs:
- **Debug Output** dans Visual Studio
- **Device Logs** via adb (si configuré)
- **Console** des messages d'erreur en app

