# ⚡ QUICK START - Samsung A55 Deployment

## 🎯 5-Minute Setup

### Step 1: Get Your PC IP (1 minute)
```powershell
ipconfig
```
**Find the "IPv4 Address" line - copy the number like `192.168.1.45`**

### Step 2: Update MauiProgram.cs (2 minutes)
File: `Cynapharm-Mobile/MauiProgram.cs`

Find this line:
```csharp
return "https://192.168.1.X:7777/";
```

**Replace `X` with your last IP octet**, e.g.:
```csharp
return "https://192.168.1.45:7777/";  // If your PC IP ends in .45
```

### Step 3: Prepare Device (1 minute)
On your **Samsung A55**:
- Settings → Developer Options → **USB Debugging** ON
- Connect via USB cable to PC
- Allow USB debugging prompt on device

### Step 4: Build & Deploy (1 minute)
In Visual Studio:
1. Select your Samsung A55 from device dropdown
2. Press **F5** (Start Debugging)
3. Wait for app to install and launch

---

## ✅ Expected Results

**Success:**
- App launches on device
- Login screen appears
- Enter credentials and login
- Dashboard or main screen loads ✅

**Failure Points & Fixes:**

| Error | Fix |
|-------|-----|
| "Device not found" | `adb kill-server && adb start-server` |
| "USB Debugging disabled" | Enable in Settings → Developer Options |
| "Connection timeout" | Check IP in MauiProgram.cs is correct |
| "Connection refused" | Start backend: `dotnet run` in Gateway folder |
| "Invalid credentials" | Backend is working, check username/password |

---

## 🔍 Verify Setup

```powershell
# Check device connection
adb devices

# Check backend is running
curl https://localhost:7777/api/health -SkipCertificateCheck

# View device logs in real-time
adb logcat | findstr "Cynapharm"
```

---

## 📱 Samsung A55 Requirements
- ✅ Android 14+ (One UI 6+)
- ✅ USB Debugging enabled
- ✅ Connected to same Wi-Fi network as PC
- ✅ 100MB free storage

---

## 🚀 You're Ready!

Your app configuration is now set for physical Samsung A55 device.

**Files modified:**
- `Cynapharm-Mobile/MauiProgram.cs` - IP address configured

**Next:** Just replace the `X` and deploy! 🎉
