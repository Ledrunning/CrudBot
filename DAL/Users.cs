using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.DAL
{
    public class Users
    {
       public int Id { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
        
       public Users()
       {

       }
       public Users (int id, string fn, string ln)
       {
           Id = id;
           FirstName = fn;
           LastName = ln;
       }
    }
}
