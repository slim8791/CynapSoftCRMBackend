using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuration JSON
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Services Ocelot + TON Authentification (Trés important pour la sécurité)
builder.Services.AddOcelot(builder.Configuration);
//builder.AddAppAuthentication();

var app = builder.Build();




app.MapGet("/", () => "CynapCRM Gateway is Running!");

// 3. Utiliser la version moderne 'await'
await app.UseOcelot();

app.Run();
