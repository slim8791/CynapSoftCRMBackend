# CynapCRM Gateway - Monster ASP Hosting Guide

## Overview
Your Ocelot API Gateway is now configured for hosting on Monster ASP's free plan. The configuration automatically switches between local development and production (Monster ASP) environments.

## What Changed

### 1. **Program.cs Updates**
- Added support for environment-specific Ocelot configuration files
- Enabled HTTPS redirection in production environments
- Kept HTTP-only mode for local development

### 2. **New Configuration File: ocelot.Production.json**
- Uses your Monster ASP service URLs (all hosted on runasp.net)
- Routes all traffic over HTTPS with proper port 443 configuration
- Configured upstream base URL to your gateway URL

## Your Monster ASP Service URLs

Your services are already hosted on Monster ASP at:
- **Auth API**: https://cynapharmauth.runasp.net
- **Document API**: https://cynapharmdocs.runasp.net
- **Field API**: https://cynapharmfields.runasp.net
- **Inventory API**: https://cynapharminventories.runasp.net
- **Order API**: https://cynapharmorders.runasp.net
- **Product API**: https://cynapharmproducts.runasp.net
- **Gateway**: https://cynapharmgateway.runasp.net

## Deployment Steps

### Step 1: Publish the Gateway
```powershell
# From the project directory
dotnet publish -c Release -o ./publish
```

### Step 2: Upload to Monster ASP
1. Log in to your Monster ASP control panel
2. Create/navigate to your gateway application site (cynapharmgateway.runasp.net)
3. Use FTP or the file manager to upload the contents of the `publish` folder
4. Ensure the publish directory is set to the application root

### Step 3: Verify Monster ASP Configuration
1. Ensure your web.config is present (should be auto-generated)
2. Check that the Application Pool is running .NET 9
3. Verify HTTPS is enabled (should be automatic on runasp.net)

### Step 4: Test the Gateway
Once deployed, test with:
```bash
curl https://cynapharmgateway.runasp.net/
# Should return: "CynapCRM Gateway is Running!"

# Test an auth endpoint
curl -X POST https://cynapharmgateway.runasp.net/auth/login \
  -H "Content-Type: application/json"
```

## Environment Configuration

### Local Development
- Uses `ocelot.json` (with localhost:port references)
- No HTTPS redirection
- Allows HTTP traffic

### Production (Monster ASP)
- Uses `ocelot.Production.json` (with runasp.net URLs)
- Enforces HTTPS
- Proper port 443 routing

### Staging (Optional)
To add a staging environment, create `ocelot.Staging.json` and deploy with:
```powershell
set ASPNETCORE_ENVIRONMENT=Staging
```

## Important: Free Plan Considerations

### Memory Limitations
- Free plan has memory constraints
- The gateway itself is lightweight, but monitor downstream service responsiveness
- If services time out, you may need to upgrade to a paid plan

### Sleep Mode
- Monster ASP free plan may sleep inactive sites
- First request after sleep takes 5-10 seconds
- Solution: Consider keeping services active with periodic health checks

### CPU Throttling
- Free plan has CPU throttling limits
- High-traffic scenarios may require paid hosting
- Consider load balancing if traffic exceeds limits

## Configuration for High Availability

If you need better uptime, consider these upgrades:

1. **Upgrade to Starter Plan** - Better memory/CPU allocation
2. **Add Caching** - Use Ocelot's built-in caching to reduce downstream calls
3. **Health Checks** - Monitor and alert on service health

## Troubleshooting

### 502 Bad Gateway
- Verify downstream services are running on Monster ASP
- Check that service URLs in ocelot.Production.json match your actual service URLs
- Ensure HTTPS certificates are valid for all services

### 503 Service Unavailable
- Free plan may have gone to sleep - wait and retry
- Consider upgrading to prevent sleep mode

### SSL Certificate Issues
- Monster ASP automatically handles SSL
- If issues persist, manually renew certificates in the control panel

### Slow Response Times
- Check Monster ASP resource usage
- Review Ocelot logs in Event Viewer on Monster ASP
- Consider caching frequently accessed endpoints

## Monitoring & Logs

### On Monster ASP
1. Access logs via Control Panel → Logs
2. Check Event Viewer for ASP.NET application errors
3. Review HTTP status codes for common failures

### Local Debugging
Run locally first to verify functionality:
```powershell
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

## Next Steps

1. **Test locally** with all your downstream services running
2. **Deploy to Monster ASP** using the publish folder
3. **Monitor** the gateway logs for the first 24 hours
4. **Set up alerts** for any service failures or high error rates

## Support Resources

- **Ocelot Documentation**: https://ocelot.readthedocs.io/
- **Monster ASP Help**: Check your hosting provider's documentation
- **ASP.NET Core Deployment**: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/

---

**Configuration Last Updated**: [Today's Date]
**Target Environment**: Monster ASP Free Plan
**.NET Version**: 9.0
