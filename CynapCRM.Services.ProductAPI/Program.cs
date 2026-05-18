using AutoMapper;
using CynapCRM.Services.ProductApi.Extensions;
using CynapCRM.Services.ProductAPI.Data;
using CynapCRM.Services.ProductAPI.Service;
using CynapCRM.Services.ProductAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Si vous avez cette ligne :
builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

});
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

builder.AddAppAuthentication();

var app = builder.Build();
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowAll");

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
                Console.WriteLine(">>> Migration appliqu�e avec succ�s !");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($">>> Erreur lors de la migration : {ex.Message}");
        }
    }
}