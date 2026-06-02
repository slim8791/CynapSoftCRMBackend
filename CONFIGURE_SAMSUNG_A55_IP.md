# 🔧 SAMSUNG A55 - CONFIGURATION SETUP

## ⚠️ IMPORTANT: Update Network Address BEFORE Deploying!

Your app is currently configured to connect to: **`https://192.168.1.X:7777/`**

You MUST replace the **X** with your actual PC IP address.

---

## 📱 Step 1: Find Your PC's IP Address

### On Your Windows PC (Run in PowerShell):
```powershell
ipconfig
```

### Look for the "Wireless LAN adapter Wi-Fi:" section:
```
Ethernet adapter Ethernet:
   Connection-specific DNS Suffix  . : domain.com
   IPv4 Address. . . . . . . . . . . : 192.168.1.45
   Subnet Mask . . . . . . . . . . . : 255.255.255.0
   Default Gateway . . . . . . . . . : 192.168.1.1
```

**Your IPv4 Address is the one starting with `192.168.x.x` or `10.x.x.x`**

### Examples:
- ✅ `192.168.1.45` (most common)
- ✅ `192.168.0.50` (alternative)
- ✅ `10.0.0.100` (some networks)
- ❌ `127.0.0.1` (only for localhost - won't work on physical device)
- ❌ `10.0.2.2` (only for Android emulator, not physical device)

---

## 🔧 Step 2: Update MauiProgram.cs

### Current Configuration (BEFORE):
```csharp
#if ANDROID
    return "https://192.168.1.X:7777/";  // Replace X!
#endif
```

### Example - If Your PC IP is `192.168.1.45`:
```csharp
#if ANDROID
    return "https://192.168.1.45:7777/";
#endif
```

### Example - If Your PC IP is `192.168.0.100`:
```csharp
#if ANDROID
    return "https://192.168.0.100:7777/";
#endif
```

---

## 🔌 Step 3: Verify Connectivity

### Test 1: Check Backend is Running
```powershell
# On your PC, verify the API Gateway is accessible
curl https://localhost:7777/api/health -SkipCertificateCheck

# Expected response: 200 OK (or your health endpoint response)
```

### Test 2: Check Network Connection from Device

**On your Samsung A55:**
1. Open **Settings** → **About Phone** → **Status** (or similar)
2. Note the Wi-Fi IP assigned to your device (e.g., `192.168.1.50`)
3. Go to **Chrome** and type: `https://192.168.1.45:7777` (your PC IP)
4. You should see either:
   - ✅ The API response
   - ✅ A certificate warning (this is OK in debug mode)
   - ❌ "Connection timeout" = Wrong IP or firewall blocked

### Test 3: Enable Firewall Access
```powershell
# On Windows, ensure port 7777 is open for your network
New-NetFirewallRule -DisplayName "HTTPS 7777 - MAUI Dev" `
    -Direction Inbound -Action Allow -Protocol TCP -LocalPort 7777 `
    -Profile "Private,Public" -Enabled $true
```

---

## 📦 Step 4: Build and Deploy to Samsung A55

### Via Visual Studio:
1. **Connect** your Samsung A55 via USB cable
2. **Enable USB Debugging** on the device:
   - Settings → Developer Options → USB Debugging ✅
3. **Verify connection:**
   ```powershell
   adb devices
   # Should show your device
   ```
4. In Visual Studio:
   - Set Configuration to **Debug**
   - Select your device from the device dropdown
   - Press **F5** to build and deploy
5. **Wait** for the app to install and launch

### Via Command Line:
```powershell
cd Cynapharm-Mobile

# Clean previous build
dotnet clean -f net10.0-android

# Build
dotnet build -f net10.0-android -c Debug

# Install on device
adb install -r bin/Debug/net10.0-android/com.companyname.cynapharmmobile-Signed.apk

# Launch app
adb shell am start -n com.companyname.cynapharmmobile/com.companyname.cynapharmmobile.MainActivity
```

---

## ✅ Verification Checklist

- [ ] Found your PC's Wi-Fi IPv4 address (e.g., 192.168.1.45)
- [ ] Updated MauiProgram.cs with correct IP (replaced X with actual number)
- [ ] Backend API Gateway is running (`dotnet run` in CynapCRM.Gateway folder)
- [ ] Windows Firewall allows port 7777
- [ ] Samsung A55 is connected via USB with USB Debugging enabled
- [ ] `adb devices` shows your device
- [ ] Built the project for Android
- [ ] App installed on device

---

## 🚀 Testing Login

1. **Launch app** on Samsung A55
2. **Enter credentials:**
   - Email: (your test account)
   - Password: (your test password)
3. **Expected results:**
   - ✅ Login successful → Dashboard appears
   - ❌ "Erreur de connexion" → Wrong IP address or API not running
   - ❌ "Email ou mot de passe incorrect" → Backend responding, wrong credentials
   - ❌ Long delay then timeout → Firewall or network issue

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| "Device not found in Visual Studio" | Restart ADB: `adb kill-server && adb start-server` |
| "USB Debugging disabled" | Enable: Settings → Developer Options → USB Debugging |
| "Connection timeout on login" | Check IP in MauiProgram.cs matches your PC |
| "Can't access 192.168.1.X" | Device on different Wi-Fi network or firewall blocked |
| "SSL certificate error" | Normal for self-signed cert in debug - network_security_config allows it |
| App crashes immediately | Check Debug Output window (View → Output) for errors |

---

## 📋 IP Address Configuration Guide

### Finding Your PC IP by Network Type:

**Wi-Fi Connection (most common):**
```powershell
ipconfig | findstr /A:2 "IPv4.*192\|IPv4.*10\|IPv4.*172"
```

**Ethernet Connection:**
```powershell
ipconfig | findstr /A:2 "Ethernet -A 5"
```

**Quick Command (shows all IPv4):**
```powershell
[System.Net.Dns]::GetHostAddresses([System.Net.Dns]::GetHostName()) | 
  Where-Object { $_.AddressFamily -eq 'InterNetwork' } | 
  ForEach-Object { $_.IPAddress }
```

---

## 💡 Pro Tips

1. **Dynamic IP?** If your PC gets different IP each restart:
   - Set static IP in router or Windows
   - Or create multiple builds with different IPs

2. **Testing Both Emulator & Device?**
   - Create a conditional constant in code
   - Build different APKs for each scenario

3. **Connection Strings**
   - Save them in user secrets instead of hardcoding
   - Implement settings UI to change API URL at runtime

4. **Certificate Issues?**
   - For production: Use valid SSL certificate
   - For dev: Current self-signed setup works well

---

## 🔄 Next Steps After Successful Login

1. Test all main features:
   - [ ] Dashboard loading
   - [ ] Viewing data lists
   - [ ] Creating/editing records
   - [ ] Uploading documents

2. Monitor device logs for errors:
   ```powershell
   adb logcat | findstr "Cynapharm"
   ```

3. Check network requests with Fiddler/Charles:
   - Monitor HTTPS calls
   - Verify authentication tokens
   - Check response times

---

**Your Configuration File Location:**
`Cynapharm-Mobile/MauiProgram.cs` - Line with `#if ANDROID` section

**Backend Gateway Location:**
`CynapCRM.Gateway/` - Start with `dotnet run --launch-profile https`

**Device SDK Location:**
`%APPDATA%\Android\sdk` (should be installed and updated)

