using CrudBot.DAL.Entitiy;
using CrudBot.Main.Model;

namespace CrudBot.Main.Abstraction;

public interface IUserService
{
    public Task FillData(CancellationToken token);
    public Task AddUserAsync(string firstName, string secondName, CancellationToken token);
    public Task<IList<User>> ReadAllUsersAsync(CancellationToken token);
    public Task EditUserByIdAsync(UserDto user, CancellationToken token);
    public Task DeleteUserByIdAsync(long id, CancellationToken token);
    public Task DeleteAllUsers(CancellationToken token);
}