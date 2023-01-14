﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using CrudBot.DAL.Entitiy;

namespace CrudBot.DAL.Repository
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private async Task<bool> ExecuteAsync(SqlCommand command, CancellationToken token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                await connection.OpenAsync(token);
                await command.ExecuteNonQueryAsync(token);

                if (command.Connection.State == ConnectionState.Open)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task CreateTable(CancellationToken cancellationToken)
        {
            const string query = @"IF NOT EXISTS (
	                                    SELECT
		                                    *
	                                    FROM
		                                    sysobjects
	                                    WHERE
		                                    name = 'CrudBotUsers'
		                                    and xtype = 'U'
                                    ) CREATE TABLE [dbo].[CrudBotUsers] (
                                            [Id] INTEGER IDENTITY(1,1) PRIMARY KEY,
                                            [FirstName] NVARCHAR(255),
                                            [LastName] NVARCHAR(255));";

            var command = new SqlCommand();
            command.CommandText = query;
            await ExecuteAsync(command, cancellationToken);
        }

        public async Task<bool> AddUserAsync(string firstName, string lastName, CancellationToken token)
        {
            var command = new SqlCommand();
            command.CommandText =
                "INSERT INTO [dbo].[CrudBotUsers] (FirstName, LastName) VALUES(@firstName, @lastName)";
            command.Parameters.Add(new SqlParameter("@firstName", firstName));
            command.Parameters.Add(new SqlParameter("@lastName", lastName));

            return await ExecuteAsync(command, token);
        }

        public async Task<bool> EditUsersAsync(User user, CancellationToken token)
        {
            var command = new SqlCommand();

            command.CommandText = @"UPDATE [dbo].[CrudBotUsers]
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
            command.CommandText = "DELETE FROM [dbo].[CrudBotUsers] WHERE Id = @Id";
            command.Parameters.Add(new SqlParameter("Id", id));

            return await ExecuteAsync(command, token);
        }

        public async Task<bool> DeleteAllAsync(CancellationToken token)
        {
            var command = new SqlCommand();
            command.CommandText = "DELETE FROM [dbo].[CrudBotUsers]";
            return await ExecuteAsync(command, token);
        }

        public async Task<IList<User>> ReadUsersAsync(CancellationToken token)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var users = new List<User>();
                const string query = @"SELECT 
                                       [Id],
                                       [FirstName],
                                       [LastName]
                                       FROM [dbo].[CrudBotUsers]";

                var command = new SqlCommand(query, connection);

                await connection.OpenAsync(token);
                var reader = await command.ExecuteReaderAsync(token);
                while (await reader.ReadAsync(token))
                {
                    users.Add(new User(Convert.ToInt64(reader["Id"]), reader["FirstName"].ToString(),
                        reader["LastName"].ToString()));
                }

                return users;
            }
        }
    }
}