using Microsoft.EntityFrameworkCore;

namespace CryptidCare.Api.Data;

public class CryptidCareDbContext : DbContext
{
    public CryptidCareDbContext(DbContextOptions<CryptidCareDbContext> options)
        : base(options)
    {
    }
}
