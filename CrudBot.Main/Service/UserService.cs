using System.Reflection;
using CrudBot.DAL.Contracts;
using CrudBot.DAL.Entity;
using CrudBot.Exceptions.Exceptions;
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
        try
        {
            await _userRepository.AddUserAsync(firstName, secondName, token);
        }
        catch (Exception e)
        {
            _logger.LogError("Error add user {e}", e);
        }
    }

    public async Task<IList<User>> ReadAllUsersAsync(CancellationToken token)
    {
        try
        {
            return await _userRepository.ReadAllAsync(token);
        }
        catch (Exception e)
        {
            _logger.LogError("Error read all users {e}", e);
            throw new CrudBotException("Error read all users", e);
        }
    }

    public async Task EditUserByIdAsync(UserDto user, CancellationToken token)
    {
        try
        {
            var entity = new User(user.Id, user.FirstName, user.LastName);
            await _userRepository.EditUsersAsync(entity, token);
        }
        catch (Exception e)
        {
            _logger.LogError("Error editing user {e}", e);
        }
    }

    public async Task DeleteUserByIdAsync(long id, CancellationToken token)
    {
        try
        {
            await _userRepository.DeleteUserAsync(id, token);
        }
        catch (Exception e)
        {
            _logger.LogError("Error delete user by Id: {Id}, {e}", id, e);
        }
    }

    public async Task DeleteAllUsers(CancellationToken token)
    {
        try
        {
            await _userRepository.DeleteAllAsync(token);
        }
        catch (Exception e)
        {
            _logger.LogError("Error delete all users: {e}", e);
        }
    }

    public async Task<User> GetUserAsync(long id, CancellationToken token)
    {
        try
        {
            return await _userRepository.GetUserAsync(id, token);
        }
        catch (Exception e)
        {
            _logger.LogError("Error to getting user by Id: {e}", e);
        }

        return new User(default, default, default);
    }
}