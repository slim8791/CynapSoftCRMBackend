using CynapCRM.Services.FieldAPI.Data;
using CynapCRM.Services.FieldAPI.Extensions;
using CynapCRM.Services.FieldAPI.Service;
using CynapCRM.Services.FieldAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using CynapCRM.MessageBus.Extensions;
using CynapCRM.Services.FieldAPI.Consumers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Entrez 'Bearer ' suivi de votre token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            }, new List<string>()
        }
    });
});
builder.AddAppAuthentication();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IKPIService, KPIService>();
builder.Services.AddScoped<IObjectifService, ObjectifService>();
builder.Services.AddScoped<IPlanningService, PlanningService>();
builder.Services.AddScoped<IRapportService, RapportService>();
builder.Services.AddScoped<IRegionService, RegionService>();
builder.Services.AddScoped<IVisiteService, VisiteService>();
builder.Services.AddCynapMessageBus(builder.Configuration, x =>
{
    x.AddConsumer<StockDistributedConsumer>();
    x.AddConsumer<UserCreatedConsumer>(); 

});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
applyMigrations();
app.Run();

void applyMigrations()
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
                Console.WriteLine(">>> Migration appliquée avec succès !");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Erreur lors de la migration : {ex.Message}");
        }
    }
}
