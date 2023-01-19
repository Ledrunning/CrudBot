namespace CrudBot.Main.Model;

public class PersonDto
{
    public List<Person>? User { get; set; }
}

public class Person
{
    public string? Name { get; set; }
    public string? LastName { get; set; }
}