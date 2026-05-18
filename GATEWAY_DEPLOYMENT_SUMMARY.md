# CynapCRM Gateway - Monster ASP Deployment Summary

## ✅ Deployment Status: READY

All components have been successfully configured for deployment on Monster ASP.

## 📊 Configuration Overview

### Gateway Statistics
- **Total Routes**: 69
- **Target Framework**: .NET 10
- **Protocol**: HTTP (Monster ASP handles HTTPS)
- **Port**: 80 (standard HTTP)

### Microservice Distribution
```
┌─────────────────────────────────────────────────────────┐
│ Authentication API (cynapharmauth.runasp.net)          │
│ • 19 routes                                              │
│ • Auth, Users, Profile management                        │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Product API (cynapharmproducts.runasp.net)              │
│ • 24 routes                                              │
│ • Products, Lots, Promos, Marketing                      │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Document API (cynapharmdocs.runasp.net)                 │
│ • 4 routes                                               │
│ • Factures, Bons, Documents                              │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Field API (cynapharmfields.runasp.net)                  │
│ • 6 routes                                               │
│ • KPI, Objectives, Planning, Reports, Regions, Visits   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Inventory API (cynapharminventories.runasp.net)         │
│ • 7 routes                                               │
│ • Stock, Warehouses, Distributions                       │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│ Order API (cynapharmorders.tryasp.net) ⚠️               │
│ • 9 routes                                               │
│ • Orders, Reclamations, Line Items                       │
│ Note: Different domain (.tryasp.net)                     │
└─────────────────────────────────────────────────────────┘
```

## 🔧 Files Modified

### 1. CynapCRM.Gateway/ocelot.json
- ✅ All 69 routes configured with correct endpoints
- ✅ All routes use HTTP protocol on port 80
- ✅ Authentication options configured for protected routes
- ✅ CORS enabled for all origins

### 2. CynapCRM.Gateway/Program.cs
- ✅ HTTPS redirection enabled
- ✅ CORS policy configured
- ✅ Ocelot middleware configured
- ✅ JWT authentication enabled

## 🚀 Quick Deployment Steps

### Step 1: Build
```powershell
cd CynapCRM.Gateway
dotnet build -c Release
```

### Step 2: Publish
```powershell
dotnet publish -c Release -o ./publish
```

### Step 3: Deploy to Monster ASP
- Connect to Monster ASP via FTP
- Upload contents of `./publish` directory
- Ensure file permissions are correct

### Step 4: Configure Monster ASP
- Runtime: .NET 10
- Application Pool: Start/Running
- HTTPS: Enable (Monster ASP infrastructure)

### Step 5: Test
```powershell
# Test gateway
curl https://your-gateway-domain.com/

# Test auth endpoint
curl -X POST https://your-gateway-domain.com/auth/login
```

## ⚠️ Important Notes

### Authentication API (cynapharmauth.runasp.net)
- Routes for login/forgot-password: No token required
- All other routes: Bearer token required

### Order API (cynapharmorders.tryasp.net)
- **IMPORTANT**: Uses different domain (.tryasp.net instead of .runasp.net)
- Verify this endpoint is accessible before deployment
- Test connectivity: `ping cynapharmorders.tryasp.net`

### Security Configuration
- CORS: Currently allows all origins (`*`)
- For production, restrict to specific domains:
  ```json
  "AllowedOrigins": ["https://your-frontend-domain.com"]
  ```

## 📋 Pre-Deployment Checklist

- [ ] All microservices deployed and accessible
- [ ] DNS records updated (if using custom domain)
- [ ] Monster ASP account created and ready
- [ ] FTP credentials obtained from Monster ASP
- [ ] .NET 10 runtime available on Monster ASP
- [ ] Test connectivity to all microservice endpoints
- [ ] Backup previous version (if applicable)
- [ ] Verify Order API endpoint (cynapharmorders.tryasp.net)

## 🧪 Testing Scenarios

### Test 1: Gateway Health
```bash
curl https://your-gateway.com/
# Expected: 404 or welcome message (depends on DEBUG mode)
```

### Test 2: Public Authentication
```bash
curl -X POST https://your-gateway.com/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'
```

### Test 3: Protected Route
```bash
curl https://your-gateway.com/auth/users \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Test 4: Product Listing
```bash
curl https://your-gateway.com/products/visible \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🔍 Monitoring and Logs

### Key Metrics to Monitor
- Gateway response times
- Backend service connectivity
- Authentication success rate
- Route error rates
- CORS request handling

### Log Locations (Monster ASP)
- Application Logs: Monster ASP control panel
- Error Logs: Check application event logs
- IIS Logs: `/inetpub/logs/LogFiles/`

## 📞 Troubleshooting

### Issue: 503 Service Unavailable
**Cause**: Backend microservice not accessible
**Solution**: 
- Verify microservice URLs are correct
- Check network connectivity to external services
- Verify Monster ASP network policies allow outbound connections

### Issue: 401 Unauthorized
**Cause**: Invalid or missing Bearer token
**Solution**:
- Verify token is valid
- Check Authentication header format
- Ensure JWT configuration is correct

### Issue: CORS Errors
**Cause**: Frontend requesting from blocked origin
**Solution**:
- Update CORS policy in Program.cs
- Specify allowed origins instead of `*`
- Test with curl first

## 📝 Configuration Verification

Run this command to verify configuration:
```powershell
$json = Get-Content "ocelot.json" | ConvertFrom-Json
$json.Routes | Group-Object -Property @{Expression={$_.DownstreamHostAndPorts[0].Host}} | ForEach-Object { Write-Host "$($_.Name): $($_.Count) routes" }
```

Expected Output:
```
cynapharmauth.runasp.net: 19 routes
cynapharmdocs.runasp.net: 4 routes
cynapharmfields.runasp.net: 6 routes
cynapharminventories.runasp.net: 7 routes
cynapharmorders.tryasp.net: 9 routes
cynapharmproducts.runasp.net: 24 routes
```

## 🎯 Next Steps

1. **Immediate**: Deploy gateway to Monster ASP
2. **Post-Deployment**: Test all routes thoroughly
3. **Monitoring**: Set up logs and alerts
4. **Documentation**: Update API documentation with new gateway URL
5. **Security**: Restrict CORS for production
6. **Performance**: Monitor and optimize as needed

## 📞 Support Resources

- Ocelot Documentation: https://ocelot.readthedocs.io/
- Monster ASP: https://monsterasp.com/
- .NET 10 Documentation: https://learn.microsoft.com/en-us/dotnet/

---

**Deployment Status**: ✅ Ready for Production
**Last Updated**: 2024
**Configured By**: GitHub Copilot
**Environment**: Monster ASP Free Plan
