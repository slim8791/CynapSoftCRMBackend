# ✅ Samsung A55 Deployment Verification Checklist

## 📋 Pre-Deployment (Do This FIRST)

### 1️⃣ Backend Setup
- [ ] Navigate to `CynapCRM.Gateway` folder
- [ ] Run: `dotnet run --launch-profile https`
- [ ] Wait until console shows: "Application started. Press Ctrl+C to shut down"
- [ ] Test in browser: `https://localhost:7777/swagger` (should load Swagger UI)

### 2️⃣ Get Your PC IP Address
- [ ] Open PowerShell
- [ ] Run: `ipconfig`
- [ ] Find line: `IPv4 Address . . . . . . . . . . : 192.168.X.X`
- [ ] **Write it down:** `192.168.___.___ ` ← This is YOUR PC IP
- [ ] Make sure device is on **SAME Wi-Fi network**

### 3️⃣ Update Code
- [ ] Open: `Cynapharm-Mobile/MauiProgram.cs`
- [ ] Find: `return "https://192.168.1.X:7777/";`
- [ ] Replace with your actual IP, e.g.: `return "https://192.168.1.45:7777/";`
- [ ] **Save file** (Ctrl+S)

### 4️⃣ Prepare Samsung A55
- [ ] Turn on device
- [ ] Settings → About Phone → Build Number (tap 7 times)
- [ ] Settings → Developer Options → **USB Debugging** = ON
- [ ] Settings → Developer Options → **Wireless Debugging** = ON (optional)
- [ ] Settings → Developer Options → **Stay Awake** = ON (optional, helpful)

### 5️⃣ Connect Device
- [ ] Connect Samsung A55 via USB cable to your PC
- [ ] On device: "Allow USB debugging from this computer?" → **OK**
- [ ] Device should show "Connected" in Windows
- [ ] Open PowerShell: `adb devices` → Shows your device

---

## 🏗️ Build & Deploy

### 6️⃣ Build for Android
```powershell
cd Cynapharm-Mobile

# Option A: Via Command Line
dotnet build -f net10.0-android -c Debug

# Option B: Via Visual Studio
# Build → Build Solution (Ctrl+Shift+B)
```

- [ ] Build completes successfully (check Output window)
- [ ] No error messages

### 7️⃣ Deploy to Device
**Option A: Visual Studio (Recommended)**
- [ ] Set Configuration dropdown to **Debug**
- [ ] Select your Samsung A55 from device dropdown
- [ ] Press **F5** or click "Start Debugging" button
- [ ] Wait for "Installation finished" message
- [ ] App launches on device

**Option B: Command Line**
```powershell
# Install APK
adb install -r bin/Debug/net10.0-android/com.companyname.cynapharmmobile-Signed.apk

# Launch app
adb shell am start -n com.companyname.cynapharmmobile/com.companyname.cynapharmmobile.MainActivity
```

---

## 🧪 Testing (Post-Deployment)

### 8️⃣ Verify App Launched
- [ ] App icon appears on Samsung A55 home screen
- [ ] Cynapharm app launches
- [ ] **Login screen** appears

### 9️⃣ Test Login
- [ ] Email field: Enter your test email (e.g., `test@example.com`)
- [ ] Password field: Enter your test password
- [ ] Press **Login** button
- [ ] **Expected:** Dashboard loads successfully ✅

### Possible Outcomes:

**✅ SUCCESS - You should see:**
- Login screen → Enter credentials → Dashboard/Orders/Products page loads
- No error messages
- Data displays correctly

**❌ FAILURE - "Erreur de connexion. Vérifiez votre connexion internet."**
- **Cause:** Wrong IP address in MauiProgram.cs
- **Fix:** Check your PC IP with `ipconfig` and update MauiProgram.cs
- **Then:** Rebuild and deploy again

**❌ FAILURE - "Email ou mot de passe incorrect"**
- **Cause:** Backend IS responding, credentials are wrong
- **Fix:** Check username/password with backend or database
- **Good news:** App is connecting correctly!

**❌ FAILURE - Long delay then timeout**
- **Cause:** Device can't reach PC IP
- **Fix 1:** Both on same Wi-Fi network?
- **Fix 2:** Windows Firewall blocking port 7777?
  ```powershell
  New-NetFirewallRule -DisplayName "HTTPS 7777" -Direction Inbound `
    -Action Allow -Protocol TCP -LocalPort 7777
  ```
- **Fix 3:** Backend API actually running?

**❌ FAILURE - App crashes immediately**
- **Cause:** Unknown error in app code
- **Fix:** Check Debug Output window
- **Show:** `View → Output` and look for exceptions

---

## 🔧 Troubleshooting Commands

```powershell
# View real-time device logs
adb logcat

# Filter for app-specific logs
adb logcat | findstr "Cynapharm"

# Get device info
adb shell getprop ro.build.version.release

# Uninstall app
adb uninstall com.companyname.cynapharmmobile

# View installed packages
adb shell pm list packages | findstr cynapharm

# Clear app data
adb shell pm clear com.companyname.cynapharmmobile

# Check device storage
adb shell df
```

---

## ⚙️ Configuration Verification

| Item | Status | Location |
|------|--------|----------|
| Backend HTTPS Port | 7777 | CynapCRM.Gateway |
| App API URL | `https://[PC-IP]:7777/` | MauiProgram.cs |
| Android Min API | 21 | Cynapharm-Mobile.csproj |
| Device API Level | 34+ (A55) | ✅ Compatible |
| SSL Bypass (Debug) | Enabled | HttpClientHandler |
| Network Permissions | Granted | AndroidManifest.xml |
| Network Config | Configured | network_security_config.xml |

---

## 📊 Device Information

**Samsung A55:**
- **Model:** Samsung Galaxy A55
- **Android Version:** 14 (One UI 6.0)
- **Min SDK:** API 34
- **Target SDK:** API 34+
- **Architecture:** ARM64
- **Supports:** HTTPS with self-signed certificates (in debug)

**Your App:**
- **Target:** net10.0-android
- **Min Android:** API 21 (supports A55 API 34 ✅)
- **Debug Build:** SSL validation bypassed ✅
- **Network Access:** Full HTTPS support ✅

---

## 🎯 Success Indicators

You'll know everything is working when:

1. ✅ `adb devices` shows your Samsung A55
2. ✅ Backend API runs without errors
3. ✅ App installs without errors
4. ✅ Login screen appears on device
5. ✅ Login succeeds with valid credentials
6. ✅ Dashboard/main content loads
7. ✅ No error messages in Debug Output

---

## 🚨 Common Mistakes to Avoid

❌ **Mistake:** Using `10.0.2.2` for physical device
✅ **Fix:** Use actual PC IP from `ipconfig`

❌ **Mistake:** Port 5555 (HTTP) instead of 7777 (HTTPS)
✅ **Fix:** Always use 7777

❌ **Mistake:** Backend not running
✅ **Fix:** Keep `dotnet run --launch-profile https` running in another PowerShell

❌ **Mistake:** Device on different Wi-Fi network
✅ **Fix:** Check both on same network, or use IP directly accessible from device

❌ **Mistake:** Firewall blocking port 7777
✅ **Fix:** Allow inbound TCP 7777 in Windows Firewall

---

## 📞 If Something Goes Wrong

1. **Check the logs:**
   ```powershell
   adb logcat | findstr "Cynapharm"
   ```

2. **Verify connection:**
   ```powershell
   adb devices
   ```

3. **Check backend:**
   ```powershell
   curl https://localhost:7777/api/health -SkipCertificateCheck
   ```

4. **Verify IP:**
   ```powershell
   ipconfig
   ```

5. **Check firewall:**
   ```powershell
   Get-NetFirewallRule | findstr "7777"
   ```

---

## ✨ You're All Set!

Your .NET MAUI app is configured for Samsung A55.

**Last thing:** Just update that `X` in the IP address and you're ready to deploy! 🚀

