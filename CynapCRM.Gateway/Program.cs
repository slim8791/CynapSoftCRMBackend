using CynapCRM.Gateway.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot config
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Ocelot
builder.Services.AddOcelot(builder.Configuration);

// JWT for the Gateway
builder.AddAppAuthentication();

var app = builder.Build();

// app.UseHttpsRedirection(); // Removed to allow HTTP in development

#if DEBUG
app.MapGet("/", () => "CynapCRM Gateway is Running!");
#endif

// Use CORS
app.UseCors("AllowAll");

// Set Referrer-Policy header
app.Use(async (context, next) =>
{
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();