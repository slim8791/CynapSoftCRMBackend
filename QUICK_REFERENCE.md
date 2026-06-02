# 🚀 CynapCRM Gateway - Quick Reference

## ✅ Status: DEPLOYMENT READY

```
✓ Configuration: Complete
✓ Build: Successful
✓ Routes: 69 configured
✓ Endpoints: 6 microservices mapped
✓ Documentation: Complete
```

## 🎯 Microservice Endpoints

| Service | Endpoint | Routes |
|---------|----------|--------|
| 🔐 Auth | http://cynapharmauth.runasp.net | 19 |
| 📦 Products | http://cynapharmproducts.runasp.net | 24 |
| 📄 Docs | http://cynapharmdocs.runasp.net | 4 |
| 🏢 Fields | http://cynapharmfields.runasp.net | 6 |
| 📊 Inventory | http://cynapharminventories.runasp.net | 7 |
| 🛒 Orders | http://cynapharmorders.tryasp.net ⚠️ | 9 |

## 📦 Deployment Package Contents

```
CynapCRM.Gateway/
├── Program.cs (✓ HTTPS enabled)
├── ocelot.json (✓ 69 routes configured)
├── Extensions/
│   └── WebApplicationBuilderExtensions.cs
└── [other files...]
```

## 🚀 Deploy in 3 Steps

### 1️⃣ Build
```powershell
dotnet build -c Release
```

### 2️⃣ Publish
```powershell
dotnet publish -c Release -o ./publish
```

### 3️⃣ Upload to Monster ASP
- Use FTP to upload `./publish` contents
- Configure .NET 10 runtime
- Enable HTTPS

## ⚡ Quick Test

```bash
# Health check
curl https://your-gateway.com/

# Login (public)
curl -X POST https://your-gateway.com/auth/login

# Protected route (requires token)
curl https://your-gateway.com/auth/users \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 📋 Important Checklist

```
Before Deployment:
☐ All microservices accessible (test URLs)
☐ Order API verified (.tryasp.net)
☐ Monster ASP account ready
☐ .NET 10 runtime confirmed
☐ FTP credentials obtained
☐ Backup previous version

After Deployment:
☐ Gateway loads without errors
☐ Test auth endpoint (no token)
☐ Test protected endpoint (with token)
☐ Monitor logs for errors
☐ Test product/order endpoints
☐ Verify response times
```

## ⚠️ Critical Notes

### Order API Uses Different Domain
```
❌ WRONG: cynapharmorders.runasp.net
✅ CORRECT: cynapharmorders.tryasp.net
```

### CORS Currently Allows All Origins
```json
"AllowedOrigins": ["*"]  // Restrict for production
```

### All Routes Use HTTP:80
```
Gateway ──(HTTP:80)──> Microservices
                ↓
        Monster ASP Infrastructure
                ↓
        (HTTPS to External Clients)
```

## 🔗 Route Categories

### 🔐 Authentication (19 routes)
- `/auth/login` - Public
- `/auth/register` - Public
- `/auth/users` - Protected (Bearer token)
- `/auth/{everything}` - Dynamic

### 📦 Products (24 routes)
- `/products/visible` - Browse products
- `/products/search` - Search
- `/products/lots` - Lot management
- `/products/{everything}` - All operations

### 📄 Documents (4 routes)
- `/documents/factures` - Invoices
- `/documents/bons-livraison` - Delivery notes
- `/documents/bons-commandes` - Purchase orders

### 🏢 Fields (6 routes)
- `/fields/kpi` - Key performance indicators
- `/fields/plannings` - Planning data
- `/fields/visites` - Visit records

### 📊 Inventory (7 routes)
- `/inventory/stock` - Stock management
- `/inventory/warehouses` - Warehouse data
- `/inventory/stock-movements` - Movement tracking

### 🛒 Orders (9 routes)
- `/orders` - Order listing
- `/orders/lignes` - Line items
- `/orders/reclamations` - Complaints/Claims

## 📞 Troubleshooting Quick Links

| Issue | Solution |
|-------|----------|
| 503 Service Unavailable | Check microservice URLs & connectivity |
| 401 Unauthorized | Verify Bearer token is valid |
| CORS Errors | Update allowed origins |
| Slow Response | Check microservice performance |
| Connection Timeout | Verify Monster ASP outbound rules |

## 📚 Documentation Files

```
✓ GATEWAY_DEPLOYMENT_SUMMARY.md - Complete overview
✓ DEPLOYMENT_GUIDE_CORRECTED.md - Detailed instructions
✓ QUICK_REFERENCE.md - This file
```

## 🎯 Success Criteria

After deployment, verify:
- [ ] Gateway responds to HTTP requests
- [ ] Public endpoints work (login, forgot-password)
- [ ] Protected endpoints require Bearer token
- [ ] All 6 microservices accessible
- [ ] Response times < 500ms
- [ ] No errors in logs
- [ ] HTTPS enforced by Monster ASP

## 💡 Pro Tips

1. **Enable compression** in Monster ASP for faster responses
2. **Set up monitoring** for backend service connectivity
3. **Implement rate limiting** to prevent abuse
4. **Use browser DevTools** to verify CORS headers
5. **Monitor JWT token expiration** issues
6. **Test with Postman** before integrating with frontend

## 🔐 Security Notes

- Disable `AllowAnyOrigin` in production
- Verify HTTPS is enforced
- Check JWT token validation
- Monitor for unauthorized access attempts
- Log all API calls for auditing

---

**Version**: 1.0
**Status**: ✅ READY FOR DEPLOYMENT
**Target**: Monster ASP Free Plan
**Framework**: .NET 10
