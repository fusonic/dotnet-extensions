using Example.Database.Data;
using Example.Database.Domain;
using Microsoft.EntityFrameworkCore;

namespace Example.Database;

public class PersonService
{
    private readonly AppDbContext dbContext;
    public PersonService(AppDbContext dbContext) => this.dbContext = dbContext;

    public async Task<List<Person>> GetPersons() => await dbContext.Persons.ToListAsync();
}