using CET254_EventManager.Data;
using Microsoft.EntityFrameworkCore;

namespace CET254_EventManager.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateContext()
    {
        // Tests use a unique EF Core InMemory database so production LocalDB is never touched.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"CET254_EventManager_Tests_{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
