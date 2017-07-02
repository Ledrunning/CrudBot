using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.DAL
{
    public static class UserManager
    {
        static string DBaddress = ConfigurationManager.ConnectionStrings["DBConection"].ConnectionString;

        // Connection handler for all classes;
        private static bool NonQuery(SqlCommand comm)
        {
            int result = -1;
            using (SqlConnection con = new SqlConnection(DBaddress))
            {
                comm.Connection = con;
                con.Open();
                comm.ExecuteNonQuery();
            }
            return (result == 1)?true:false;
        }


        // Add users method;

        public static bool AddUsers(Users s)
        {
            SqlCommand comm = new SqlCommand();
            comm.CommandText = "INSERT INTO [dbo].[Users] (FirstName, LastName) VALUES(@fn,@ln)";
            comm.Parameters.Add(new SqlParameter("fn", s.FirstName));
            comm.Parameters.Add(new SqlParameter("@ln", s.LastName));

            return NonQuery(comm);
        }

        // Edit users method;
        public static bool EditUsers(Users s)
        {
            if (s.Id == default(int))
                throw new Exception("Пользователь не записан!");
            SqlCommand comm = new SqlCommand();
            comm.CommandText = "UPDATE [dbo].[Users] SET FirstName = @fn, LastName = @ ln WHERE Id = @Id";
            comm.Parameters.Add(new SqlParameter("fn", s.FirstName));
            comm.Parameters.Add(new SqlParameter("fn", s.LastName));
            comm.Parameters.Add(new SqlParameter("Id", s.Id));

            return NonQuery(comm);
        }

        // Delete Users From DB;
        public static bool DeleteUsers(Users s)
        {
            if (s.Id == default(int))
                throw new Exception("Не записали Id пользователя!");

            SqlCommand comm = new SqlCommand();
            comm.CommandText = "DELETE FROM [dbo].[Users] WHERE Id = @Id";
            comm.Parameters.Add(new SqlParameter("Id", s.Id));

            return NonQuery(comm);
        }

        // Delete all users from data base;
        public static bool DeleteAll()
        {
            SqlCommand comm = new SqlCommand();
            comm.CommandText = "DELETE FROM [dbo].[Users]";
            
            return NonQuery(comm);

        }

        // Read from DB;
        public static List<Users> ReadDataFromDB()
        {

            using (SqlConnection con = new SqlConnection(DBaddress))
            {
                List<Users> users = new List<Users>();
                string select = "SELECT * FROM Users";
                SqlCommand cmd = new SqlCommand(select, con);
                try
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        users.Add(new Users(Convert.ToInt32(reader["Id"]), reader["FirstName"].ToString(), reader["LastName"].ToString()));
                    }
                }

               catch
                {

                }

                return users;
            }
        }

        // Search by name;
        public static List<Users> SearchUsersByFirstName(string fn)
        {
            using (SqlConnection con = new SqlConnection(DBaddress))
            {
                List<Users> usersByFirstName = new List<Users>();
                string select = "SELECT * FROM Users WHERE FirstName = @fn";
                SqlCommand cmd = new SqlCommand(select, con);
                cmd.Connection = con;
                cmd.CommandText = select;
                con.Open();
                cmd.Parameters.Add(new SqlParameter("fn", fn));
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usersByFirstName.Add(new Users(Convert.ToInt32(reader["Id"]), reader["FirstName"].ToString(), reader["LastName"].ToString()));
                }

                //con.Close();
                return usersByFirstName;
            }
        }

        // Filling database from random arrays;
        public static void FillDataBase(string[] fn, string[] ln)
        {
            Random rnd = new Random();
            for (int i = 0; i < fn.Length-1; i++)
            {
                AddUsers(new Users(rnd.Next(0, 8), fn[i], ln[i]));
            }
        }
    } 
}
