using Example.Database.Data;
using Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore;
using Fusonic.Extensions.UnitTests.Adapters.PostgreSql;

namespace Example.Database.Tests;

public class TestDbTemplateCreator : ITestDbTemplateCreator
{
    public void Create(string connectionString)
    {
        //The connection string contains the test db name that gets used as prefix.
        var dbName = PostgreSqlUtil.GetDatabaseName(connectionString);
 
        //Drop all databases that may still be there from previously stopped tests.
        PostgreSqlUtil.Cleanup(connectionString, dbPrefix: dbName!);
 
        //Create the template
        PostgreSqlUtil.CreateTestDbTemplate<AppDbContext>(connectionString, o => new AppDbContext(o), seed: c => new TestDataSeed(c).Seed());
    }
}