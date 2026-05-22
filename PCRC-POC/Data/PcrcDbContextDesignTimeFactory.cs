using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PCRC.Data;

/// Design-time factory used by `dotnet ef` (migrations, scaffolding). The connection
/// string is never opened during migration generation, so a placeholder is sufficient.
public class PcrcDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PcrcDbContext>
{
    public PcrcDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PcrcDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PcrcDb;Trusted_Connection=True;")
            .Options;

        return new PcrcDbContext(options);
    }
}
