using AutoMapper;
using CynapCRM.Services.ProductApi.Extensions;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Service;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ILotService, LotService>();
builder.Services.AddScoped<IMarkettingService, MarkettingService>();
builder.Services.AddScoped<IPromoService, PromoService>();



builder.AddAppAuthentication();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

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