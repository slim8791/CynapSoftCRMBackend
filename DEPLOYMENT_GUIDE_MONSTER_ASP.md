# CynapCRM Gateway - Monster ASP Deployment Guide

## Overview
The CynapCRM Gateway has been updated to be deployed on Monster ASP with all microservices pointing to their respective published endpoints.

## Changes Made

### 1. **ocelot.json Configuration**
All API Gateway routes have been updated from local development endpoints to external production endpoints:

#### Route Mapping:

| Service | Endpoint |
|---------|----------|
| **Authentication API** | http://cynapharmauth.runasp.net |
| **Document API** | http://cynapharmdocs.runasp.net |
| **Field API** | http://cynapharmfields.runasp.net |
| **Inventory API** | http://cynapharminventories.runasp.net |
| **Order API** | http://cynapharmorders.runasp.net |
| **Product API** | http://cynapharmproducts.runasp.net |

#### Route Details:

**Authentication Routes:**
- `POST /auth/login` → AuthAPI
- `POST /auth/forgot-password` → AuthAPI
- `GET,POST,PUT,DELETE /auth/{everything}` → AuthAPI

**Product Routes:**
- `/products/*` → ProductAPI
- `/marketting/*` → ProductAPI

**Document Routes:**
- `/documents/factures/*` → DocAPI
- `/documents/bons-livraison/*` → DocAPI
- `/documents/bons-commandes/*` → DocAPI
- `/documents/*` → DocAPI

**Field Routes:**
- `/fields/kpi/*` → FieldAPI
- `/fields/objectifs/*` → FieldAPI
- `/fields/plannings/*` → FieldAPI
- `/fields/rapports/*` → FieldAPI
- `/fields/regions/*` → FieldAPI
- `/fields/visites/*` → FieldAPI

**Inventory Routes:**
- `/inventory/distributions/*` → InventoryAPI
- `/inventory/stock/*` → InventoryAPI
- `/inventory/inventory-business/*` → InventoryAPI
- `/inventory/warehouses/*` → InventoryAPI
- `/inventory/stock-movements/*` → InventoryAPI
- `/inventory/stocks-promotionnels/*` → InventoryAPI
- `/inventory/stocks-delegue/*` → InventoryAPI

**Order Routes:**
- `/orders` → OrderAPI
- `/orders/reclamations` → OrderAPI
- `/orders/lignes/*` → OrderAPI
- `/orders/reclamations/*` → OrderAPI
- `/orders/*` → OrderAPI

### 2. **Program.cs Updates**
- **Enabled HTTPS Redirection**: Uncommented `app.UseHttpsRedirection()` to support secure HTTPS connections on Monster ASP
- All other configurations remain unchanged to maintain functionality

### 3. **Protocol Changes**
- Changed from `https` to `http` for downstream connections to microservices
- Using standard HTTP port `80` instead of development ports (7000-7005)
- Monster ASP handles HTTPS at the gateway level

## Deployment Steps

### Prerequisites:
1. Ensure all microservices are deployed and accessible:
   - http://cynapharmauth.runasp.net
   - http://cynapharmdocs.runasp.net
   - http://cynapharmfields.runasp.net
   - http://cynapharminventories.runasp.net
   - http://cynapharmorders.runasp.net
   - http://cynapharmproducts.runasp.net

2. Verify network connectivity to all external endpoints

### Deployment Process:

1. **Build the Gateway Project**
   ```bash
   dotnet build CynapCRM.Gateway
   ```

2. **Publish to Monster ASP**
   ```bash
   dotnet publish CynapCRM.Gateway -c Release -o ./publish
   ```

3. **Upload to Monster ASP**
   - Use FTP or Monster ASP's deployment interface to upload the published files

4. **Configure Monster ASP Settings**
   - Set .NET Runtime to .NET 9
   - Configure application pool for the gateway

5. **Verify Deployment**
   - Test the gateway health: `GET /` (in DEBUG mode)
   - Test an authentication endpoint: `POST /auth/login`
   - Monitor logs for any connectivity issues

## Testing

### Health Check:
```bash
curl https://your-gateway-domain.com/
```
Expected response: "CynapCRM Gateway is Running!"

### Sample API Call:
```bash
curl -X POST https://your-gateway-domain.com/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"user","password":"pass"}'
```

## Important Notes

1. **CORS Policy**: Currently allows all origins (`AllowAnyOrigin`). Consider restricting this in production:
   ```csharp
   options.AddPolicy("AllowAll", builder =>
   {
       builder.WithOrigins("https://your-frontend-domain.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
   });
   ```

2. **HTTPS Redirection**: Enabled for production safety. Ensure Monster ASP is configured for HTTPS.

3. **Bearer Token Authentication**: All protected routes require Bearer token authentication.

4. **Referrer Policy**: Set to `strict-origin-when-cross-origin` for security.

## Troubleshooting

### Connection Issues:
- Verify all microservice endpoints are accessible from Monster ASP
- Check firewall rules on Monster ASP
- Ensure DNS resolution works for all external endpoints

### Authentication Issues:
- Verify JWT configuration matches your identity provider
- Check token expiration and refresh logic

### Performance:
- Monitor response times from the gateway
- Consider implementing caching if needed
- Enable compression for large responses

## Configuration Files Modified

1. `CynapCRM.Gateway/ocelot.json` - Updated all routes with external endpoints
2. `CynapCRM.Gateway/Program.cs` - Enabled HTTPS redirection

## Rollback Plan

If issues occur after deployment:
1. Keep a backup of the previous build
2. Redeploy the previous version to Monster ASP
3. Review logs to identify the issue
4. Update configuration and redeploy

---

**Last Updated**: 2024
**Target Framework**: .NET 9
**Deployment Platform**: Monster ASP (Free Plan)
