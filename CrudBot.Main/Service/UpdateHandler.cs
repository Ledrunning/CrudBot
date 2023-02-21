using CrudBot.Main.Abstraction;
using CrudBot.Main.Helpers;
using CrudBot.Weather.Contract;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using CrudBot.Main.Model;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
namespace CrudBot.Main.Service;

public class UpdateHandler : IUpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly IOpenWeatherRestService _openWeatherService;
    private readonly IUserService _userService;
    private static long _id;
    private bool _isWeather;
    private bool _isUserDelete;
    private bool _isUserAdd;
    private bool _isGetUser;
    private bool _isUserEdit;

    public UpdateHandler(ITelegramBotClient botClient,
        IUserService userService,
        ILogger<UpdateHandler> logger, 
        IOpenWeatherRestService openWeatherService)
    {
        _botClient = botClient;
        _userService = userService;
        _logger = logger;
        _openWeatherService = openWeatherService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient _, Update update, CancellationToken token)
    {
        var handler = update switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            { Message: { } message } => BotOnMessageReceived(message, token),
            { EditedMessage: { } message } => BotOnMessageReceived(message, token),
            { CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, token),
            { InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, token),
            { ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult,
                token),
            _ => UnknownUpdateHandlerAsync(update)
        };

        await handler;
    }

    public async Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);

        // Cooldown in case of network connection error
        if (exception is RequestException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receive message type: {MessageType}", message.Type);
        if (message.Text is not { } messageText)
        {
            return;
        }

        Task<Message>? action = null;
        if (messageText.Split(' ')[0] == "/fill_data")
        {
            action = FillDataAsync(_botClient, _userService, message, cancellationToken);
        }
        else if (messageText.Split(' ')[0] == "/add_person")
        {
            _isUserAdd = true;
            action = _botClient.SendTextMessageAsync(message.Chat.Id, "Enter person's Name and Last name with a space between!", cancellationToken: cancellationToken);
        }
        else if (_isUserAdd)
        {
            action = AddPersonAsync(_botClient, _userService, message, cancellationToken);
            _isUserAdd = false;
        }
        else if (messageText.Split(' ')[0] == "/get_persons")
        {
            action = GetAllPersonsAsync(_botClient, _userService, message, cancellationToken);
        }
        else if (messageText.Split(' ')[0] == "/edit_person")
        {
            _isGetUser = true;
            action = _botClient.SendTextMessageAsync(message.Chat.Id, "Enter person Id!", cancellationToken: cancellationToken);
        }
        else if (_isGetUser)
        {
            _ = long.TryParse(message.Text, out var id);

            _id = id;

            var user = await _userService.GetUserAsync(id, cancellationToken);
            action = _botClient.SendTextMessageAsync(message.Chat.Id, $"{user.FirstName} {user.LastName}", cancellationToken: cancellationToken);
            _isGetUser = false;
            _isUserEdit = true;
        }
        else if (_isUserEdit)
        {
            action = EditPersonAsync(_botClient, _userService, message, cancellationToken);
            _isUserEdit = false;
        }
        else if (messageText.Split(' ')[0] == "/delete_person")
        {
            _isUserDelete = true;
            action = _botClient.SendTextMessageAsync(message.Chat.Id,"Enter person Id!", cancellationToken: cancellationToken);
        }
        else if (_isUserDelete)
        {
            action = DeletePersonByIdAsync(_botClient, _userService, message, cancellationToken);
            _isUserDelete = false;
        }
        else if (messageText.Split(' ')[0] == "/delete_all")
        {
            action = DeleteAllPersonsAsync(_botClient, _userService, message, cancellationToken);
        }
        else if (messageText.Split(' ')[0] == "/get_weather")
        {
            _isWeather = true;
            action = EnterCityNameAsync(_botClient, message, cancellationToken);
        }
        else if (_isWeather)
        {
            action = GetWeatherAsync(_botClient, _openWeatherService, message, cancellationToken);
            _isWeather = false;
        }
        else if (messageText.Split(' ')[0] == "/throw")
        {
            throw new IndexOutOfRangeException();
        }
        else
        {
            action = Usage(_botClient, message, cancellationToken);
        }

        var sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken)
        {
            const string usage = "Hi! I'm Simple Crud Boy! You can use the following commands:\n" +
                                 "/fill_data       - Put data to database from user.json file\n" +
                                 "/add_person      - Add person into database\n" +
                                 "/get_persons     - Get all persons from database\n" +
                                 "/edit_person     - Edit person in database\n" +
                                 "/delete_person   - Delete person by Id\n" +
                                 "/delete_all      - Delete all persons from database\n" +
                                 "/get_weather     - Get current weather";

            return await botClient.SendTextMessageAsync(
                message.Chat.Id,
                usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }

        static async Task<Message> FillDataAsync(ITelegramBotClient botClient, IUserService userService,
            Message message,
            CancellationToken token)
        {
            await userService.FillData(token);

            return await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Data has been filled!",
                cancellationToken: token);
        }

        static async Task<Message> GetAllPersonsAsync(ITelegramBotClient botClient, IUserService userService,
            Message message,
            CancellationToken token)
        {
            var users = await userService.ReadAllUsersAsync(token);
            foreach (var user in users)
            {
                await botClient.SendTextMessageAsync(
                    message.Chat.Id,
                    $"Id:{user.Id}-{user.FirstName} {user.LastName}",
                    cancellationToken: token);
            }

            return await botClient.SendTextMessageAsync(message.Chat.Id,
                "Done!", cancellationToken: token);
        }

        static async Task<Message> AddPersonAsync(ITelegramBotClient botClient, IUserService userService,
            Message message, CancellationToken token)
        {
            var validatedName = ValidateName(message, token);

            if (!validatedName.IsNameMatch)
            {
                return await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Please, enter a valid name and last name!", cancellationToken: token);
            }

            await userService.AddUserAsync(validatedName.FirstName, validatedName.LastName, token);
            return await botClient.SendTextMessageAsync(message.Chat.Id,
                $"User with Name:{validatedName.FirstName} {validatedName.LastName} has been added successfully", cancellationToken: token);

        }

        static async Task<Message> EditPersonAsync(ITelegramBotClient botClient, IUserService userService,
            Message message,
            CancellationToken token)
        {
            var validatedName = ValidateName(message, token);

            if (!validatedName.IsNameMatch)
            {
                return await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Please, enter a valid name and last name!", cancellationToken: token);
            }

            await userService.EditUserByIdAsync(new UserDto
            {
                Id = validatedName.Id,
                FirstName = validatedName.FirstName,
                LastName = validatedName.LastName
            }, token);

            return await botClient.SendTextMessageAsync(message.Chat.Id,
                $"User with Name:{validatedName.FirstName} {validatedName.LastName} has been edited successfully", cancellationToken: token);
        }

        static async Task<Message> DeletePersonByIdAsync(ITelegramBotClient botClient, IUserService userService,
            Message message,
            CancellationToken token)
        {
            _ = long.TryParse(message.Text, out var id);
            await userService.DeleteUserByIdAsync(id, token);
            return await botClient.SendTextMessageAsync(message.Chat.Id,
                $"User with Id:{id} has been deleted successfully", cancellationToken: token);
        }

        static async Task<Message> DeleteAllPersonsAsync(ITelegramBotClient botClient, IUserService userService,
            Message message,
            CancellationToken token)
        {
            await userService.DeleteAllUsers(token);
            return await botClient.SendTextMessageAsync(message.Chat.Id,
                "All clear!", cancellationToken: token);
        }

        static async Task<Message> EnterCityNameAsync(ITelegramBotClient botClient, Message message, CancellationToken token)
        {
            return await botClient.SendTextMessageAsync(message.Chat.Id,
                "Enter the city name!", cancellationToken: token);
        }

        static async Task<Message> GetWeatherAsync(ITelegramBotClient botClient, IOpenWeatherRestService openWeatherService,
            Message message,
            CancellationToken token)
        {
            var isMatch = message.Text != null && Regex.IsMatch(message.Text, @"^[a-zA-Z]+$");

            if (!isMatch)
            {
                return await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Please, enter a valid city name!", cancellationToken: token);
            }

            var weather = await openWeatherService.GetWeatherFromOpenWeatherApi(message.Text, token);

            var result =
                $"Weather in {message.Text}: T={TemperatureConverter.ConvertKelvinToTemperature(weather.Main.Temp):#.##}," +
                $" H%= {weather.Main.Humidity}, P={weather.Main.Pressure}";

            return await botClient.SendTextMessageAsync(message.Chat.Id,
                result, cancellationToken: token);

        }
    }

    private static UserDto ValidateName(Message message, CancellationToken token)
    {
        var regex = new Regex(@"^[A-Z][a-z]+\s[A-Z][a-z]+$");

        if (message.Text == null || !regex.IsMatch(message.Text))
        {
            return new UserDto
            {
                IsNameMatch = false
            };
        }

        var match = regex.Match(message.Text);

        var result = match.Groups[0].Value.Split(" ");
        var firstName = result[0];
        var lastName = result[1];
        
        return new UserDto
        {
            Id = _id,
            IsNameMatch = true,
            FirstName = firstName,
            LastName = lastName
        };
    }

    // Process Inline Keyboard callback data
    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

        await _botClient.AnswerCallbackQueryAsync(
            callbackQuery.Id,
            $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);

        await _botClient.SendTextMessageAsync(
            callbackQuery.Message!.Chat.Id,
            $"Received {callbackQuery.Data}",
            cancellationToken: cancellationToken);
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    private async Task BotOnInlineQueryReceived(InlineQuery inlineQuery, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline query from: {InlineQueryFromId}", inlineQuery.From.Id);

        InlineQueryResult[] results =
        {
            // displayed result
            new InlineQueryResultArticle(
                "1",
                "TgBots",
                new InputTextMessageContent("hello"))
        };

        await _botClient.AnswerInlineQueryAsync(
            inlineQuery.Id,
            results,
            0,
            true,
            cancellationToken: cancellationToken);
    }

    private async Task BotOnChosenInlineResultReceived(ChosenInlineResult chosenInlineResult,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received inline result: {ChosenInlineResultId}", chosenInlineResult.ResultId);

        await _botClient.SendTextMessageAsync(
            chosenInlineResult.From.Id,
            $"You chose result with Id: {chosenInlineResult.ResultId}",
            cancellationToken: cancellationToken);
    }
}