using CrudBot.DAL1.Entitiy;

namespace CrudBot.DAL1.Contracts;

public interface IUserRepository
{
    Task CreateTable(CancellationToken cancellationToken);
    Task<bool> AddUserAsync(string firstName, string lastName, CancellationToken token);
    Task<bool> EditUsersAsync(User user, CancellationToken token);
    Task<bool> DeleteUsersAsync(int id, CancellationToken token);
    Task<bool> DeleteAllAsync(CancellationToken token);
    Task<IList<User>> ReadUsersAsync(CancellationToken token);
}