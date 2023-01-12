using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace DAL
{
    public class UserManager
    {
        private static readonly string ConnectionString =
            ConfigurationManager.ConnectionStrings["DBConection"].ConnectionString;

        private static async Task<bool> ExecuteAsync(SqlCommand command, CancellationToken token)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                command.Connection = connection;
                await connection.OpenAsync(token);
                await command.ExecuteNonQueryAsync(token);

                if (command.Connection.State == ConnectionState.Open)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> AddUsersAsync(User user, CancellationToken token)
        {
            var command = new SqlCommand();
            command.CommandText = "INSERT INTO [dbo].[Users] (FirstName, LastName) VALUES(@firstName, @lastName)";
            command.Parameters.Add(new SqlParameter("@firstName", user.FirstName));
            command.Parameters.Add(new SqlParameter("@lastName", user.LastName));

            return await ExecuteAsync(command, token);
        }

        public async Task<bool> EditUsersAsync(User user, CancellationToken token)
        {
            var command = new SqlCommand();

            command.CommandText = @"UPDATE [dbo].[Users]
                                    SET FirstName = @firstName, LastName = @lastName 
                                    WHERE Id = @Id";

            command.Parameters.Add(new SqlParameter("firstName", user.FirstName));
            command.Parameters.Add(new SqlParameter("lastName", user.LastName));
            command.Parameters.Add(new SqlParameter("Id", user.Id));

            return await ExecuteAsync(command, token);
        }

        public async Task<bool> DeleteUsersAsync(int id, CancellationToken token)
        {
            var command = new SqlCommand();
            command.CommandText = "DELETE FROM [dbo].[Users] WHERE Id = @Id";
            command.Parameters.Add(new SqlParameter("Id", id));

            return await ExecuteAsync(command, token);
        }

        public async Task<bool> DeleteAllAsync(CancellationToken token)
        {
            var command = new SqlCommand();
            command.CommandText = "DELETE FROM [dbo].[Users]";
            return await ExecuteAsync(command, token);
        }

        public static IList<User> ReadUsersAsync()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                var users = new List<User>();
                const string query = @"SELECT 
                                       [Id],
                                       [FirstName],
                                       [LastName]
                                       FROM [dbo].[Users]";

                var command = new SqlCommand(query, connection);

                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User(Convert.ToInt64(reader["Id"]), reader["FirstName"].ToString(),
                        reader["LastName"].ToString()));
                }

                return users;
            }
        }

        public async Task FillDataBase(string[] fn, string[] ln, CancellationToken token)
        {
            var rnd = new Random();
            for (var i = 0; i < fn.Length - 1; i++)
            {
                await AddUsersAsync(new User(rnd.Next(0, 8), fn[i], ln[i]), token);
            }
        }
    }
}