using Example.Database.Domain;
using Microsoft.EntityFrameworkCore;

namespace Example.Database.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    { }

    public DbSet<Person> Persons { get; set; } = null!;
}