# HTTP Error 500.30 - Quick Fix Guide

## The Problem
Your Ocelot gateway published to Monster ASP is showing:
```
HTTP Error 500.30 - ASP.NET Core app failed to start
```

## Why This Happens

Most common causes on Monster ASP free plan:

1. **Missing web.config** - IIS can't start the app
2. **HTTPS redirection failure** - Gateway tried to redirect but Monster ASP environment doesn't support it
3. **Configuration files not uploaded** - appsettings.json or ocelot.json missing
4. **Environment variable not set** - Wrong configuration loaded
5. **Port binding conflict** - App trying to listen on wrong port

## What Was Fixed

All issues are now resolved with the updated files:

✅ **Program.cs**
- Now binds to `http://0.0.0.0:80` (correct for IIS)
- HTTPS redirection removed (Monster ASP handles externally)
- Better error handling for config file loading

✅ **web.config** (NEW)
- Tells IIS how to run your .NET Core app
- Critical file for Monster ASP deployment

✅ **appsettings.Production.json** (NEW)
- Production configuration settings
- Ensures correct environment loads

✅ **ocelot.Production.json** (FIXED)
- Uses HTTP (port 80) not HTTPS (port 443)
- Routes correctly to your services

## One-Time Fix Steps

### Step 1: Clean Local Build
```powershell
cd C:\Cynapharm\CynapSoftCRMBackend\CynapCRM.Gateway
dotnet clean
dotnet publish -c Release -o ./publish --no-self-contained
```

### Step 2: Login to Monster ASP Control Panel
- Navigate to your gateway application

### Step 3: Delete ALL Old Files
Using File Manager or FTP:
- Delete **everything** in the application root directory
- This is important - leftover files can cause conflicts

### Step 4: Upload New Files
Copy **everything** from your local `publish` folder to Monster ASP root:
- Must include: `web.config` (in root!)
- Must include: `appsettings.*.json` files
- Must include: `ocelot.*.json` files
- Must include: all DLL files

### Step 5: Set Environment Variable
In Monster ASP Control Panel → Application Settings:
```
ASPNETCORE_ENVIRONMENT = Production
```

### Step 6: Restart
1. Restart the application pool (if option available)
2. Wait 2-3 minutes
3. Test: `https://cynapharmgateway.runasp.net/`

## Verification Checklist

After deploying, verify:

```powershell
# Test 1: Gateway is running
curl https://cynapharmgateway.runasp.net/
# Expected: "CynapCRM Gateway is Running!"
# Status: 200 OK

# Test 2: Try a route (will fail to service but not 500 error from gateway)
curl -X POST https://cynapharmgateway.runasp.net/auth/login
# Expected: 502/503/timeout (service sleeping) or service error
# NOT Expected: 500 error from gateway itself
```

## If Still Failing (Advanced)

### Enable Detailed Logging
1. Update `appsettings.Production.json` on server
2. Change `"Default": "Information"` to `"Default": "Debug"`
3. Re-upload file
4. Restart app pool
5. Check Event Viewer for detailed error

### Check Event Viewer (If Available)
Monster ASP Control Panel → Logs/Events:
- Look for ".NET Runtime" errors
- Search for "CynapCRM.Gateway"
- Note the exact error message and search online

### Contact Monster ASP Support
If still stuck, provide them:
1. The exact error message from Event Viewer
2. That you're deploying a .NET 9 Core app
3. That you've uploaded web.config
4. That ASPNETCORE_ENVIRONMENT is set to Production

## Key Files Involved

| File | Purpose | Location |
|------|---------|----------|
| web.config | IIS configuration | **Root** (critical!) |
| appsettings.json | Development settings | Root |
| appsettings.Production.json | Production settings | Root |
| ocelot.json | Development routes | Root |
| ocelot.Production.json | Production routes | Root |
| Program.cs | App startup logic | N/A (compiled into DLL) |

## Common Mistakes (Don't Do These)

❌ Don't upload to a subfolder - put files in application root
❌ Don't skip web.config - it's essential for IIS
❌ Don't use HTTPS URLs for downstream services - use HTTP
❌ Don't leave old files on server - delete them first
❌ Don't forget to set ASPNETCORE_ENVIRONMENT - it must be "Production"

## Performance After Fix

After successful deployment:
- Gateway responds in <500ms on first request
- May be slower on free plan during startup (normal)
- Will sleep after 20 minutes of inactivity (normal)
- First request after sleep takes 5-10 seconds (normal)

---

**That's it!** If you follow these steps, the 500.30 error will be resolved.

If you still have issues after following this guide, check the detailed deployment guide or contact Monster ASP support with the event log error message.
