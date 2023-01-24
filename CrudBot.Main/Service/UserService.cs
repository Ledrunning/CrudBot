using System.Reflection;
using CrudBot.DAL.Contracts;
using CrudBot.DAL.Entitiy;
using CrudBot.Main.Abstraction;
using CrudBot.Main.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CrudBot.Main.Service;

internal class UserService : IUserService
{
    private static readonly string? JsonFilePath = Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location);

    private readonly ILogger<UpdateHandler> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ILogger<UpdateHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task FillData(CancellationToken token)
    {
        try
        {
            var jsonData = await File.ReadAllTextAsync(Path.Combine(JsonFilePath!, "users.json"), token);
            var userDto = JsonConvert.DeserializeObject<PersonDto>(jsonData);

            foreach (var user in userDto.User)
            {
                await _userRepository.AddUserAsync(user.Name!, user.LastName!, token);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Error loading data into database {e}", e);
        }
    }

    public async Task AddUserAsync(string firstName, string secondName, CancellationToken token)
    {
        await _userRepository.AddUserAsync(firstName, secondName, token);
    }

    public async Task<IList<User>> ReadAllUsersAsync(CancellationToken token)
    {
        return await _userRepository.ReadAllAsync(token);
    }

    public async Task EditUserByIdAsync(UserDto user, CancellationToken token)
    {
        var entity = new User(user.Id, user.FirstName, user.LastName);
        await _userRepository.EditUsersAsync(entity, token);
    }

    public async Task DeleteUserByIdAsync(long id, CancellationToken token)
    {
        await _userRepository.DeleteUserAsync(id, token);
    }

    public async Task DeleteAllUsers(CancellationToken token)
    {
        await _userRepository.DeleteAllAsync(token);
    }
}