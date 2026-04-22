using CynapCRM.Gateway.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Charger config Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Ocelot
builder.Services.AddOcelot(builder.Configuration);

// JWT pour le Gateway
builder.AddAppAuthentication();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();

app.Run();