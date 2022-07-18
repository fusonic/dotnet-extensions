namespace Example.Database.Domain;

public class Person
{
    public Person(string name) => Name = name;

    public int Id { get; set; }
    public string Name { get; set; }
}