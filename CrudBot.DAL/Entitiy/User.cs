namespace CrudBot.DAL.Entitiy
{
    public record User
    {
        public User(long id, string? firstName, string? lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}