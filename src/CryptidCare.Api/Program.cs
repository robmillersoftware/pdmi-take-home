using CryptidCare.Api.Adjudication;
using CryptidCare.Api.Adjudication.Rules;
using CryptidCare.Api.Data;
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
