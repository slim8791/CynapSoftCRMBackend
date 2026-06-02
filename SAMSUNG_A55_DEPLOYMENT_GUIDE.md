# ✅ Samsung A55 Connection Checklist - .NET MAUI Setup

## 📋 Pre-Deployment Requirements

### 1. **Device Preparation**
- [ ] Samsung A55 is connected to the same Wi-Fi network as your development PC
- [ ] Developer Mode is enabled:
  - Settings → About Phone → Build Number (tap 7 times)
  - Settings → Developer Options → USB Debugging (enabled)
  - Settings → Developer Options → Wireless debugging (optional but helpful)
- [ ] Battery level is above 20%
- [ ] Screen timeout is set to maximum (Settings → Display → Screen Timeout → 10 minutes)

### 2. **PC Prerequisites**
- [ ] Visual Studio 2026 is updated to latest version
- [ ] Android SDK Tools are installed:
  - Android SDK Platform 21 (API 21 - minimum for this app)
  - Android SDK Platform 34-35 (recommended for A55)
  - Android Build Tools (latest)
  - Android Emulator (if testing on emulator too)
- [ ] ADB (Android Debug Bridge) is installed and configured
  - Command: `adb devices` (should show connected device)

---

## 🔧 Configuration Verification

### Android Configuration Checklist

**AndroidManifest.xml** ✅
```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application
        android:allowBackup="true"
        android:icon="@mipmap/appicon"
        android:roundIcon="@mipmap/appicon_round"
        android:supportsRtl="true"
        android:networkSecurityConfig="@xml/network_security_config">
    </application>
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.INTERNET" />
</manifest>
```
- [ ] INTERNET permission present
- [ ] ACCESS_NETWORK_STATE permission present
- [ ] networkSecurityConfig referenced

**network_security_config.xml** ✅
```xml
<?xml version="1.0" encoding="utf-8"?>
<network-security-config>
    <debug-overrides>
        <trust-anchors>
            <certificates src="system" />
            <certificates src="user" />
        </trust-anchors>
        <domain-config cleartextTrafficPermitted="true">
            <domain includeSubdomains="true">10.0.2.2</domain>
            <domain includeSubdomains="true">localhost</domain>
            <domain includeSubdomains="true">127.0.0.1</domain>
        </domain-config>
    </debug-overrides>
    <domain-config cleartextTrafficPermitted="false">
        <domain includeSubdomains="true">10.0.2.2</domain>
        <domain includeSubdomains="true">localhost</domain>
    </domain-config>
</network-security-config>
```
- [ ] Debug overrides configured (for development)
- [ ] Self-signed certificates allowed
- [ ] Cleartext traffic permitted for localhost in debug

**MauiProgram.cs - API Configuration** ✅
```csharp
private static string GetApiBaseUrl()
{
#if ANDROID
    return "https://10.0.2.2:7777/";  // Real device uses actual IP
#elif IOS
    return "https://localhost:7777/";
#else
    return "https://localhost:7777/";
#endif
}
```
- [ ] HTTPS port is correct (7777)
- [ ] SSL certificate bypass enabled for DEBUG mode
- [ ] Timeout is reasonable (30 seconds)

**Project File (Cynapharm-Mobile.csproj)** ✅
```xml
<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">21.0</SupportedOSPlatformVersion>
```
- [ ] Minimum Android API Level is 21 or lower (Samsung A55 runs API 34-35)
- [ ] Target Android is included in TargetFrameworks

---

## 🚀 Deployment Steps for Samsung A55

### Step 1: Connect Device to PC
```powershell
# Verify device connection
adb devices

# Expected output:
# List of attached devices
# emulator-5554          device
# <your-device-serial>   device
```

### Step 2: Build for Physical Device
```powershell
# Navigate to project directory
cd C:\Users\benjd\Desktop\BackendPFE\CynapSoftCRMBackend\Cynapharm-Mobile

# Build APK for Android
dotnet build -f net10.0-android -c Debug

# Or use Visual Studio:
# 1. Set Build Configuration to "Debug"
# 2. Select device from dropdown menu
# 3. Press F5 or Ctrl+F5
```

### Step 3: Deploy & Run
**Option A: Via Visual Studio**
- [ ] Device appears in device dropdown (top toolbar)
- [ ] Select your Samsung A55 from dropdown
- [ ] Press F5 (Start Debugging) or Ctrl+F5 (Start Without Debugging)
- [ ] App should install and launch on device

**Option B: Via Command Line**
```powershell
# Install APK on device
adb install -r bin/Debug/net10.0-android/com.companyname.cynapharmmobile.apk

# Launch app
adb shell am start -n com.companyname.cynapharmmobile/com.companyname.cynapharmmobile.MainActivity
```

---

## 🌐 Network Configuration for Physical Device

### Samsung A55 Wi-Fi Connection (IMPORTANT!)

Since you're using a physical device (not emulator), the IP address configuration changes:

**Current Configuration Issue:**
```csharp
#if ANDROID
    return "https://10.0.2.2:7777/";  // This is for EMULATOR ONLY!
#endif
```

❌ **10.0.2.2** is a special Android emulator IP that bridges to localhost
✅ **For physical device**, use your PC's actual IP address

### Solution: Update MauiProgram.cs for Physical Device

Create a version that detects the environment:

```csharp
private static string GetApiBaseUrl()
{
#if ANDROID
    // For physical device - use your PC's IP address
    // Replace XXX.XXX.XXX.XXX with your PC's actual IP
    return "https://192.168.1.X:7777/";  // CHANGE THIS!
#elif IOS
    return "https://localhost:7777/";
#else
    return "https://localhost:7777/";
#endif
}
```

### How to Find Your PC's IP Address

```powershell
# Windows - Find your IP address
ipconfig

# Look for IPv4 Address under "Wireless LAN adapter Wi-Fi:"
# Example output:
# IPv4 Address . . . . . . . . . . : 192.168.1.45
# Subnet Mask . . . . . . . . . . : 255.255.255.0
# Default Gateway . . . . . . . . : 192.168.1.1
```

**Your PC IP is: _________________________ (fill this in)**

---

## ✅ Pre-Deployment Verification Checklist

### Backend Requirements
- [ ] ASP.NET Core API is running on your PC
  - Command: `dotnet run --project CynapCRM.Gateway` (or use VS to run it)
  - Verify: https://localhost:7777/api/health (should return 200)
  - Or: https://your-pc-ip:7777/api/health

### Network Connectivity
- [ ] Samsung A55 can ping your PC
  ```powershell
  # On phone, ping your PC
  ping <your-pc-ip>
  ```
- [ ] Port 7777 is accessible from the device
  - Install "HTTP Request Tools" app or use curl
  - Test: `curl https://<your-pc-ip>:7777/api/health`

### SSL Certificate (Development)
- [ ] Self-signed certificate is installed/trusted
  - Android: Settings → Security → Install from storage (if needed)
  - The app's network_security_config already allows self-signed in debug

### Firewall Configuration
- [ ] Windows Firewall allows port 7777
  ```powershell
  # Check firewall rule
  Get-NetFirewallRule -DisplayName "*7777*"

  # If needed, create rule:
  New-NetFirewallRule -DisplayName "HTTPS 7777" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 7777
  ```

---

## 🔍 Testing the Connection

### Test 1: API Connectivity
```powershell
# From your PC, verify API is accessible
curl https://localhost:7777/api/auth/login -k -X POST -H "Content-Type: application/json" -d '{"email":"test@example.com","password":"test"}'
```

### Test 2: Device to API (from Samsung A55)
1. Open "Chrome" or any browser on the device
2. Go to: `https://<your-pc-ip>:7777/swagger` (if Swagger is enabled)
3. You should see API documentation

### Test 3: App Login Test
1. Launch the Cynapharm app on your Samsung A55
2. Enter test credentials
3. Expected outcomes:
   - ✅ Login successful → Dashboard loads
   - ❌ "Connection error" → Check network/firewall/IP address
   - ❌ "Invalid credentials" → Backend is reachable, check login credentials

---

## 🐛 Troubleshooting Guide

| Issue | Solution |
|-------|----------|
| Device not showing in Visual Studio | Restart ADB: `adb kill-server && adb start-server` |
| "USB Debugging disabled" error | Enable in Settings → Developer Options → USB Debugging |
| Connection timeout on login | Check IP address is correct for your PC network |
| SSL certificate error | Verify network_security_config.xml is properly configured |
| App crashes on startup | Check Debug Output window for exceptions |
| "Access Denied" on port 7777 | Check Windows Firewall settings |
| Can't connect to 10.0.2.2 | You're using physical device, not emulator - use PC's real IP |

---

## 📱 Device Information - Samsung A55

**Specifications:**
- **Android Version:** 14 (One UI 6.0) or higher
- **Min API Level Supported:** 34
- **Target API Level:** 34+
- **Architecture:** ARM64-v8a (64-bit)
- **RAM:** 8GB or more
- **Storage:** Enough for app (~50-100MB)

**Your Configuration:**
- ✅ Project targets: `net10.0-android`
- ✅ Min Android API: 21 (your device has 34+)
- ✅ SSL bypass enabled for debug
- ✅ Network security configured

---

## 🚀 Quick Start Commands

```powershell
# 1. Check device connection
adb devices

# 2. Navigate to project
cd Cynapharm-Mobile

# 3. Build and deploy
dotnet build -f net10.0-android -c Debug

# 4. View device logs (real-time debugging)
adb logcat | findstr "Cynapharm"

# 5. Uninstall app if needed
adb uninstall com.companyname.cynapharmmobile

# 6. View console output from device
adb shell logcat *:S Cynapharm:V
```

---

## ✨ Final Checklist Before Deployment

- [ ] Samsung A55 is connected and shows in `adb devices`
- [ ] Backend API is running on your PC
- [ ] Windows Firewall allows port 7777
- [ ] Network security config is properly configured
- [ ] API base URL is updated with correct IP (NOT 10.0.2.2 for physical device)
- [ ] Test credentials are available
- [ ] Device has sufficient battery
- [ ] Build configuration is set to "Debug"
- [ ] Target platform is "Android" or your Samsung A55 device name

---

## 📞 Support Information

If you encounter issues, provide:
1. Device: Samsung A55
2. Android Version: (Settings → About Phone → Android Version)
3. Error message from app
4. Debug output from Visual Studio (Debug → Windows → Output)
5. Network configuration (PC IP address)
6. Output from: `adb logcat | findstr "Cynapharm"`

