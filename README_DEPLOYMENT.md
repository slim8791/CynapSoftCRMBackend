# CynapCRM Gateway - Monster ASP Deployment

> **Status**: ✅ **READY FOR DEPLOYMENT** | **Build**: ✅ Successful | **Routes**: ✅ 69 Configured

## 📋 Overview

The CynapCRM API Gateway has been fully configured for deployment on Monster ASP (free plan). All microservices are properly mapped and ready for production use.

### What Changed

- ✅ **ocelot.json**: All 69 routes updated with correct external microservice endpoints
- ✅ **Program.cs**: HTTPS redirection enabled for production
- ✅ **Configuration**: All endpoints mapped to Monster ASP hosted services
- ✅ **Documentation**: Complete deployment guides provided

## 🚀 Quick Start (Choose One)

### Option 1: PowerShell (Recommended)
```powershell
.\DEPLOY.ps1
```

### Option 2: Command Prompt
```cmd
DEPLOY.bat
```

### Option 3: Manual
```powershell
dotnet build -c Release
dotnet publish -c Release -o ./publish
# Upload ./publish contents to Monster ASP via FTP
```

## 🔗 Microservice Endpoints

| Service | Endpoint | Routes | Status |
|---------|----------|--------|--------|
| **Authentication** | `http://cynapharmauth.runasp.net` | 19 | ✅ |
| **Products** | `http://cynapharmproducts.runasp.net` | 24 | ✅ |
| **Documents** | `http://cynapharmdocs.runasp.net` | 4 | ✅ |
| **Fields** | `http://cynapharmfields.runasp.net` | 6 | ✅ |
| **Inventory** | `http://cynapharminventories.runasp.net` | 7 | ✅ |
| **Orders** | `http://cynapharmorders.tryasp.net` | 9 | ⚠️ Different Domain |

**Total Routes**: 69

## 📦 Deployment Steps

### Step 1: Build & Publish
```powershell
# Run deployment script
.\DEPLOY.ps1  # or DEPLOY.bat for Windows CMD
```

This generates the `./publish` directory with all necessary files.

### Step 2: Upload to Monster ASP
1. Connect to Monster ASP via FTP
2. Navigate to your application root
3. Upload all contents from `./publish` directory
4. Ensure proper file permissions (read/execute for application)

### Step 3: Configure Monster ASP
1. Go to Monster ASP Control Panel
2. Select your application
3. Configure settings:
   - **Runtime**: .NET 10 (or appropriate version)
   - **Start Mode**: Always running
   - **HTTPS**: Enable (recommended)
   - **Application Pool**: Start if stopped

### Step 4: Test Gateway
```bash
# Test gateway health
curl https://your-gateway-domain.com/

# Test public endpoint (login)
curl -X POST https://your-gateway-domain.com/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'

# Test protected endpoint
curl https://your-gateway-domain.com/auth/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 📂 File Structure

```
CynapCRM.Gateway/
├── Program.cs                    ✓ Updated
├── ocelot.json                   ✓ Updated (69 routes)
├── Extensions/
│   └── WebApplicationBuilderExtensions.cs
├── appsettings.json
└── ...

Deployment Files (Root):
├── DEPLOY.ps1                    ← Use this
├── DEPLOY.bat                    ← Or this
├── GATEWAY_DEPLOYMENT_SUMMARY.md
├── DEPLOYMENT_GUIDE_CORRECTED.md
├── QUICK_REFERENCE.md
└── README.md                     ← This file
```

## 🔧 Configuration Details

### ocelot.json Structure

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/auth/login",
      "UpstreamHttpMethod": [ "POST" ],
      "DownstreamPathTemplate": "/api/auth/login",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "cynapharmauth.runasp.net",
          "Port": 80
        }
      ]
    },
    // ... 68 more routes
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:7777",
    "CorsOptions": {
      "AllowCredentials": false,
      "AllowedOrigins": [ "*" ],
      "AllowedMethods": [ "*" ],
      "AllowedHeaders": [ "*" ]
    }
  }
}
```

### Program.cs Highlights

```csharp
// HTTPS Redirection
app.UseHttpsRedirection();

// CORS Configuration
app.UseCors("AllowAll");

// JWT Authentication
app.UseAuthentication();
app.UseAuthorization();

// Ocelot Middleware
await app.UseOcelot();
```

## ⚠️ Important Notes

### Order API Uses Different Domain
```
❌ INCORRECT: cynapharmorders.runasp.net
✅ CORRECT:   cynapharmorders.tryasp.net
```

### CORS Configuration
Current configuration allows all origins:
```json
"AllowedOrigins": [ "*" ]
```

**For Production**, restrict to your frontend domain:
```json
"AllowedOrigins": [ "https://your-frontend-domain.com" ]
```

Update in `Program.cs`:
```csharp
options.AddPolicy("AllowAll", builder =>
{
    builder.WithOrigins("https://your-frontend-domain.com")
           .AllowAnyMethod()
           .AllowAnyHeader();
});
```

### Protocol Configuration
- **Internal (Gateway ↔ Microservices)**: HTTP on port 80
- **External (Client ↔ Gateway)**: HTTPS (Monster ASP handles termination)

## 🧪 Testing Checklist

- [ ] Gateway responds to health checks
- [ ] Authentication endpoint works (public)
- [ ] Protected endpoints require Bearer token
- [ ] All 6 microservices are accessible
- [ ] Response times are acceptable (< 500ms)
- [ ] No errors in application logs
- [ ] HTTPS is enforced externally

## 📞 Troubleshooting

### Problem: 503 Service Unavailable
**Likely Cause**: Backend service not accessible

**Solutions**:
1. Verify microservice URLs in ocelot.json
2. Test connectivity: `ping cynapharmauth.runasp.net`
3. Check Monster ASP outbound network policies
4. Review application logs for connection errors

### Problem: 401 Unauthorized
**Likely Cause**: Invalid or missing Bearer token

**Solutions**:
1. Verify token format: `Authorization: Bearer <token>`
2. Check token expiration
3. Verify JWT configuration
4. Test with curl: `curl -H "Authorization: Bearer YOUR_TOKEN" https://...`

### Problem: CORS Errors
**Likely Cause**: Frontend from blocked origin

**Solutions**:
1. Check browser console for CORS errors
2. Update `AllowedOrigins` in configuration
3. Verify Origin header in request
4. Test with curl first (no CORS): `curl https://your-gateway.com/...`

### Problem: Slow Response
**Likely Cause**: Network latency or backend performance

**Solutions**:
1. Monitor backend service performance
2. Check network connectivity
3. Implement response caching
4. Consider connection pooling

## 📊 Route Summary

### By Service
- **Authentication**: 19 routes (user management, login, profile)
- **Products**: 24 routes (products, lots, promos, marketing)
- **Documents**: 4 routes (invoices, delivery notes, purchase orders)
- **Fields**: 6 routes (KPI, planning, reports, regions, visits)
- **Inventory**: 7 routes (stock, warehouses, distributions)
- **Orders**: 9 routes (orders, reclamations, line items)

### By Authentication
- **Public Routes**: Login, Register, Forgot Password
- **Protected Routes**: All others (require Bearer token)

### By Method
- **GET**: Retrieval operations
- **POST**: Creation operations
- **PUT**: Update operations
- **DELETE**: Deletion operations

## 🔐 Security Configuration

### Current Settings
```json
{
  "CorsOptions": {
    "AllowCredentials": false,
    "AllowedOrigins": [ "*" ],        // ⚠️ CHANGE FOR PRODUCTION
    "AllowedMethods": [ "*" ],
    "AllowedHeaders": [ "*" ]
  }
}
```

### Recommended Production Settings
```json
{
  "CorsOptions": {
    "AllowCredentials": false,
    "AllowedOrigins": [ "https://your-frontend-domain.com" ],
    "AllowedMethods": [ "GET", "POST", "PUT", "DELETE" ],
    "AllowedHeaders": [ "Content-Type", "Authorization" ]
  }
}
```

## 📚 Documentation

- **QUICK_REFERENCE.md** - Quick deployment guide
- **DEPLOYMENT_GUIDE_CORRECTED.md** - Detailed configuration guide
- **GATEWAY_DEPLOYMENT_SUMMARY.md** - Complete overview
- **DEPLOY.ps1** / **DEPLOY.bat** - Automated deployment scripts

## 🎯 Next Steps After Deployment

1. **Configure Domain**
   - Set up DNS records if using custom domain
   - Update frontend API URLs

2. **Monitor Performance**
   - Check response times
   - Monitor error rates
   - Track backend service connectivity

3. **Implement Logging**
   - Set up centralized logging
   - Monitor authentication failures
   - Track API usage

4. **Security Hardening**
   - Restrict CORS to specific domains
   - Implement rate limiting
   - Enable request logging
   - Monitor for suspicious activity

5. **Performance Optimization**
   - Enable response compression
   - Implement caching strategies
   - Monitor resource usage
   - Consider CDN for static content

## 📞 Support Resources

- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [Monster ASP Support](https://monsterasp.com/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)

## 📝 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024 | Initial deployment configuration |

---

## ✅ Deployment Verification

After deploying to Monster ASP, verify:

```powershell
# 1. Check gateway health
$response = Invoke-WebRequest -Uri "https://your-gateway-url.com/" -ErrorAction SilentlyContinue
if ($response.StatusCode -eq 200) { Write-Host "✓ Gateway is responding" }

# 2. Check microservice connectivity
$services = @(
    "cynapharmauth.runasp.net",
    "cynapharmproducts.runasp.net",
    "cynapharmdocs.runasp.net",
    "cynapharmfields.runasp.net",
    "cynapharminventories.runasp.net",
    "cynapharmorders.tryasp.net"
)

foreach ($service in $services) {
    $response = Invoke-WebRequest -Uri "http://$service/" -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) { Write-Host "✓ $service is accessible" }
}
```

---

**Status**: ✅ READY FOR DEPLOYMENT  
**Last Updated**: 2024  
**Framework**: .NET 10  
**Platform**: Monster ASP (Free Plan)  
**Routes**: 69  
**Microservices**: 6  

🚀 **You're all set! Happy deploying!**
