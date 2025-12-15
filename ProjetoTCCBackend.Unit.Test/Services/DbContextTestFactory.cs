using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;

namespace ProjetoTCCBackend.Unit.Test.Services;

public static class DbContextTestFactory
{
    public static TccDbContext Create(string dbName = "TestDb")
    {
        var options = new DbContextOptionsBuilder<TccDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new TccDbContext(options);
    }
}
