# CynapCRM Gateway - Monster ASP Deployment Guide

## Overview
The CynapCRM Gateway has been successfully updated with corrected microservice endpoints for deployment on Monster ASP. All routes now point to the correct external services.

## Updated Configuration Summary

### Microservice Endpoints

| Service | Endpoint | Routes |
|---------|----------|--------|
| **Authentication API** | http://cynapharmauth.runasp.net | 19 |
| **Product API** | http://cynapharmproducts.runasp.net | 24 |
| **Document API** | http://cynapharmdocs.runasp.net | 4 |
| **Field API** | http://cynapharmfields.runasp.net | 6 |
| **Inventory API** | http://cynapharminventories.runasp.net | 7 |
| **Order API** | http://cynapharmorders.tryasp.net | 9 |

**Total Routes:** 69

### Route Categories

#### Authentication Routes (19 routes)
- `/auth/login` - POST
- `/auth/forgot-password` - POST
- `/auth/reset-password` - POST
- `/auth/register` - POST
- `/auth/stats` - GET
- `/auth/update-profile` - PUT
- `/auth/change-password` - PUT
- `/auth/assign-role` - PUT
- `/auth/change-role` - PUT
- `/auth/disable` - PUT
- `/auth/enable` - PUT
- `/auth/delete` - DELETE
- `/auth/admin/update-password` - PUT
- `/auth/users/disabled` - GET
- `/auth/users/search` - GET
- `/auth/users/role/{role}` - GET
- `/auth/users/{id}` - GET
- `/auth/users` - GET
- `/auth/{everything}` - GET, POST, PUT, DELETE

#### Product Routes (24 routes)
- `/products/visible` - GET
- `/products/filter` - GET
- `/products/search` - GET
- `/products/categories` - GET
- `/products/available` - GET
- `/products/unavailable` - GET
- `/products/low-stock` - GET
- `/products/stock-status` - GET
- `/products/with-promotions` - GET
- `/products/expiring-lots` - GET
- `/products/top` - GET
- `/products/dashboard` - GET
- `/products/exists` - GET
- `/products/category/{cat}` - GET
- `/products/lots/*` - Various operations
- `/products/promos/{everything}` - GET, POST, PUT, DELETE
- `/products/marketting/{everything}` - GET, POST, PUT, DELETE
- `/products/{everything}` - GET, POST, PUT, DELETE
- `/marketting/{everything}` - GET, POST, PUT, DELETE

#### Document Routes (4 routes)
- `/documents/factures/{everything}` - GET, POST, PUT, DELETE
- `/documents/bons-livraison/{everything}` - GET, POST, PUT, DELETE
- `/documents/bons-commandes/{everything}` - GET, POST, PUT, DELETE
- `/documents/{everything}` - GET, POST, PUT, DELETE

#### Field Routes (6 routes)
- `/fields/kpi/{everything}` - GET, POST, PUT, DELETE
- `/fields/objectifs/{everything}` - GET, POST, PUT, DELETE
- `/fields/plannings/{everything}` - GET, POST, PUT, DELETE
- `/fields/rapports/{everything}` - GET, POST, PUT, DELETE
- `/fields/regions/{everything}` - GET, POST, PUT, DELETE
- `/fields/visites/{everything}` - GET, POST, PUT, DELETE

#### Inventory Routes (7 routes)
- `/inventory/distributions/{everything}` - GET, POST, PUT, DELETE
- `/inventory/stock/{everything}` - GET, POST, PUT, DELETE
- `/inventory/inventory-business/{everything}` - GET, POST, PUT, DELETE
- `/inventory/warehouses/{everything}` - GET, POST, PUT, DELETE
- `/inventory/stock-movements/{everything}` - GET, POST, PUT, DELETE
- `/inventory/stocks-promotionnels/{everything}` - GET, POST, PUT, DELETE
- `/inventory/stocks-delegue/{everything}` - GET, POST, PUT, DELETE

#### Order Routes (9 routes)
- `/orders` - GET
- `/orders/reclamations` - GET, POST
- `/orders/lignes/{id}` - GET, DELETE
- `/orders/lignes` - GET, POST
- `/orders/reclamations/by-commande/{id}` - GET
- `/orders/reclamations/by-client/{id}` - GET
- `/orders/lignes/{everything}` - GET, POST, PUT, DELETE
- `/orders/reclamations/{everything}` - GET, POST, PUT, DELETE
- `/orders/{everything}` - GET, POST, PUT, DELETE

## Key Changes Made

1. **Fixed Auth Routes**: All authentication routes now correctly point to `cynapharmauth.runasp.net`
2. **Fixed Product Routes**: All product routes now correctly point to `cynapharmproducts.runasp.net`
3. **Fixed Order Routes**: All order routes now correctly point to `cynapharmorders.tryasp.net` (Note: Different domain from others)
4. **Removed Port Numbers**: All routes use HTTP port 80 (Monster ASP handles HTTPS termination)
5. **Protocol**: Changed from HTTPS to HTTP for internal communication (Monster ASP gateway handles HTTPS)

## Deployment Instructions

### Prerequisites
1. All microservices must be deployed and accessible:
   - http://cynapharmauth.runasp.net
   - http://cynapharmproducts.runasp.net
   - http://cynapharmdocs.runasp.net
   - http://cynapharmfields.runasp.net
   - http://cynapharminventories.runasp.net
   - http://cynapharmorders.tryasp.net

2. Verify network connectivity to all external endpoints

### Build and Publish Steps

1. **Build the Gateway Project**
   ```powershell
   dotnet build CynapCRM.Gateway -c Release
   ```

2. **Publish for Monster ASP**
   ```powershell
   dotnet publish CynapCRM.Gateway -c Release -o ./publish
   ```

3. **Upload to Monster ASP**
   - Use FTP or Monster ASP's Web Deploy to upload files from `./publish` directory
   - Ensure you're uploading to the correct application root

4. **Monster ASP Configuration**
   - Set .NET Runtime: .NET 10 (or appropriate version)
   - Application Pool: Set to running state
   - Virtual Directory: Map root to application directory
   - HTTPS: Should be enabled by default on Monster ASP

5. **Test Connectivity**
   ```powershell
   # Test gateway health
   curl https://your-gateway-domain-on-monsterasp.com/

   # Test authentication endpoint
   curl -X POST https://your-gateway-domain-on-monsterasp.com/auth/login
   ```

## Testing Gateway Routes

### Sample Test Cases

1. **Authentication (without token)**
   ```bash
   curl -X POST https://your-gateway.com/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"test","password":"test"}'
   ```

2. **Get Products (requires token)**
   ```bash
   curl -X GET https://your-gateway.com/products/visible \
     -H "Authorization: Bearer YOUR_TOKEN_HERE"
   ```

3. **Get Orders (requires token)**
   ```bash
   curl -X GET https://your-gateway.com/orders \
     -H "Authorization: Bearer YOUR_TOKEN_HERE"
   ```

## Configuration Files Updated

- `CynapCRM.Gateway/ocelot.json` - All routes corrected with proper endpoints

## Important Notes

### CORS Configuration
Currently allows all origins. For production, restrict as needed:
```json
"CorsOptions": {
  "AllowCredentials": false,
  "AllowedOrigins": ["https://your-frontend-domain.com"],
  "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
  "AllowedHeaders": ["*"]
}
```

### Bearer Token Authentication
All routes except login and forgot-password require Bearer token authentication. Ensure your JWT configuration matches your identity provider.

### Referrer Policy
Set to `strict-origin-when-cross-origin` for security in Program.cs.

## Monitoring and Troubleshooting

### Check Gateway Logs
Monitor Monster ASP application logs for:
- Connection errors to microservices
- Authentication failures
- Route mismatches

### Verify Microservice Accessibility
```powershell
# Test connectivity to each microservice
Test-NetConnection -ComputerName cynapharmauth.runasp.net -Port 80
Test-NetConnection -ComputerName cynapharmproducts.runasp.net -Port 80
Test-NetConnection -ComputerName cynapharmdocs.runasp.net -Port 80
Test-NetConnection -ComputerName cynapharmfields.runasp.net -Port 80
Test-NetConnection -ComputerName cynapharminventories.runasp.net -Port 80
Test-NetConnection -ComputerName cynapharmorders.tryasp.net -Port 80
```

### Performance Monitoring
- Monitor response times from gateway to backend services
- Check for any timeouts or connection issues
- Consider implementing rate limiting if needed

## Rollback Procedure

If issues occur after deployment:

1. **Backup Current Config**
   ```powershell
   Copy-Item CynapCRM.Gateway/ocelot.json ocelot.json.backup
   ```

2. **Restore Previous Version**
   - Redeploy the previous working build
   - Or restore from backup

3. **Review Changes**
   - Check gateway logs
   - Verify endpoint accessibility
   - Test routes individually

## Environment-Specific Notes

### Monster ASP Specific
- ✓ HTTPS is handled by Monster ASP infrastructure
- ✓ Free plan has resource limitations
- ✓ Monitor CPU and memory usage
- ✓ Implement connection pooling for better performance
- ✓ Consider caching frequently accessed routes

### .NET 10 Compatibility
- Project targets .NET 10
- Ensure Monster ASP supports this runtime
- Update if running older .NET versions

## Support and Maintenance

### Regular Checks
- Monitor microservice uptime
- Check for version mismatches
- Review and update dependencies quarterly

### Documentation
- Keep this deployment guide updated
- Document any custom configurations
- Maintain a changelog of gateway updates

---

**Last Updated**: 2024
**Target Framework**: .NET 10
**Deployment Platform**: Monster ASP (Free Plan)
**Status**: Ready for Deployment ✓
