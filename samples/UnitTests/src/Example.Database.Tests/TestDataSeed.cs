using Example.Database.Data;
using Example.Database.Domain;

namespace Example.Database.Tests;

public class TestDataSeed
{
    private readonly AppDbContext dbContext;
    public TestDataSeed(AppDbContext dbContext) => this.dbContext = dbContext;

    public async Task Seed()
    {
        dbContext.Add(new Person("Olaf"));
        await dbContext.SaveChangesAsync();
    }
}