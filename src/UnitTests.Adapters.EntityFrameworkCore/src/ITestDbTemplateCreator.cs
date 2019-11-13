namespace Fusonic.Extensions.UnitTests.Adapters.EntityFrameworkCore
{
    /// <summary> This interface can be implemented and passed to utils like pgtestutil (and others) so they're able to create a test template via simple command line options. </summary>
    public interface ITestDbTemplateCreator
    {
        void Create(string connectionString);
    }
}