namespace CrudBot.Main.Model;

public class UserDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string .Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsNameMatch { get; set; }
}