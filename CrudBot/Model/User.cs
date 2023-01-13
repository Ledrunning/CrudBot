using System.Collections.Generic;
using Newtonsoft.Json;

namespace DAL.Model
{
    public class UserDto
    {
        public List<User> Users { get; set; }
    }


    public class User
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("lastName")] public string LastName { get; set; }
    }
}