﻿using CrudBot.DAL.Entity;

namespace CrudBot.DAL.Contracts;

public interface IUserRepository
{
    Task CreateTable(CancellationToken cancellationToken);
    Task<bool> AddUserAsync(string firstName, string lastName, CancellationToken token);
    Task<bool> EditUsersAsync(User user, CancellationToken token);
    Task<bool> DeleteUserAsync(long id, CancellationToken token);
    Task<bool> DeleteAllAsync(CancellationToken token);
    Task<IList<User>> ReadAllAsync(CancellationToken token);
    Task<User> GetUserAsync(long id, CancellationToken token);
}