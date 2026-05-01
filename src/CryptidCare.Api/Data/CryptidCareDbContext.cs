using CryptidCare.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CryptidCare.Api.Data;

public class CryptidCareDbContext : DbContext
{
    public CryptidCareDbContext(DbContextOptions<CryptidCareDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Claim> Claims => Set<Claim>();
}
