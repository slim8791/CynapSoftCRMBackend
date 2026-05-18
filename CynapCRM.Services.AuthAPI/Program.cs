using CynapCRM.Services.AuthAPI.Data;
using CynapCRM.Services.AuthAPI.Extensions; 
using CynapCRM.Services.AuthAPI.Models;
using CynapCRM.Services.AuthAPI.Service;
using CynapCRM.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<Utilisateur, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.AddAppAuthentication();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));


builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
    );
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   
        Scheme = "Bearer",                
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {votre token JWT}"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// ✅ CORS DOIT ÊTRE ICI
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

            // Check if there are any migrations that have not yet been applied.
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
                Console.WriteLine(">>> Migration appliquée avec succès !");
            }
        }
        catch (Exception ex)
        {
            // Displays the error in the console if the SQL connection fails
            Console.WriteLine($">>> Erreur lors de la migration : {ex.Message}");
        }
    }
}
