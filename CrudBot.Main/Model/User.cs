using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrudBot.Main.Model
{
    public class UserDto
    {
        public List<User>? User { get; set; }
    }

    public class User
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
    }

}