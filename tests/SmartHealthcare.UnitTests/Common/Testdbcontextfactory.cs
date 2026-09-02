using Microsoft.EntityFrameworkCore;

namespace SmartHealthcare.UnitTests.Common
{
    /// <summary>
    /// Creates isolated in-memory database contexts for tests. Each call to Create()
    /// (with no explicit name) gets its own database, so tests never leak state into
    /// one another even when they run in parallel.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static DbContextOptions<TestApplicationDbContext> BuildOptions(string databaseName)
            => new DbContextOptionsBuilder<TestApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

        public static TestApplicationDbContext Create(string? databaseName = null)
        {
            var options = BuildOptions(databaseName ?? Guid.NewGuid().ToString());
            var context = new TestApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}