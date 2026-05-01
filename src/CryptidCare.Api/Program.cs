using CryptidCare.Api.Adjudication;
using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Data;
using CryptidCare.Api.Domain.Entities;
using CryptidCare.Api.Domain.Enums;
using CryptidCare.Api.Middleware;
using CryptidCare.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<CryptidCareDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IClaimService, ClaimService>();

builder.Services.AddScoped<IValidationRule, SilverAllergyRule>();
builder.Services.AddScoped<IModifierRule, HydraMultiplierRule>();
builder.Services.AddScoped<AdjudicationEngine>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CryptidCareDbContext>();
    db.Database.Migrate();

    if (!db.Patients.Any())
    {
        db.Patients.AddRange(
            new Patient { Name = "Gary Lupine",   Species = Species.Werewolf, HeadCount = 1, IsActive = true },
            new Patient { Name = "Harriet Hydra", Species = Species.Hydra,    HeadCount = 3, IsActive = true },
            new Patient { Name = "Felix Ash",     Species = Species.Phoenix,  HeadCount = 1, IsActive = true }
        );
    }

    if (!db.Medicines.Any())
    {
        db.Medicines.AddRange(
            new Medicine { Name = "Wolfsbane Tonic", ContainsSilver = false, BaseCost = 30.00m },
            new Medicine { Name = "Silver Salve",    ContainsSilver = true,  BaseCost = 90.00m }
        );
    }

    db.SaveChanges();
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
