namespace CrudBot.DAL.Entitiy
{
    public class User
    {
        public User(long id, string fn, string ln)
        {
            Id = id;
            FirstName = fn;
            LastName = ln;
        }

        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}