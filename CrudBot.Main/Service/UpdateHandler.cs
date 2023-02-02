using CrudBot.Main.Abstraction;
using CrudBot.Main.Helpers;
using CrudBot.Weather.Contract;
using Microsoft.Extensions.Logging;
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

    public UpdateHandler(ITelegramBotClient botClient,
        IUserService userService,
        ILogger<UpdateHandler> logger, IOpenWeatherRestService openWeatherService)
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

        var action = messageText.Split(' ')[0] switch
        {
            "/fill_data" => FillDataAsync(_botClient, _userService, message, cancellationToken),
            "/get_persons" => GetAllPersonsAsync(_botClient, _userService, message, cancellationToken),
            "/delete_person" => DeletePersonByIdAsync(_botClient, _userService, message, cancellationToken),
            "/delete_all" => DeleteAllPersonsAsync(_botClient, _userService, message, cancellationToken),
            "/get_weather" => GetWeather(_botClient, _openWeatherService, message, cancellationToken),

            "/throw" => throw new IndexOutOfRangeException(),
            _ => Usage(_botClient, message, cancellationToken)
        };

        var sentMessage = await action;
        _logger.LogInformation("The message was sent with id: {SentMessageId}", sentMessage.MessageId);

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

        static async Task<Message> DeletePersonByIdAsync(ITelegramBotClient botClient, IUserService userService,
            Message message,
            CancellationToken token)
        {
            //TODO BUG! remove the stub/
            int.TryParse(message.Text, out var id);
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

        //TODO : remove hardcoded city!
        static async Task<Message> GetWeather(ITelegramBotClient botClient, IOpenWeatherRestService openWeatherService,
            Message message,
            CancellationToken token)
        {
            var weather = await openWeatherService.GetWeatherFromOpenWeatherApi("London", token);

            var result =
                $"Weather in Berlin: T={TemperatureConverter.ConvertKelvinToTemperature(weather.Main.Temp):#.##}," +
                $" H%= {weather.Main.Humidity}, P={weather.Main.Pressure}";

            return await botClient.SendTextMessageAsync(message.Chat.Id,
                result, cancellationToken: token);
        }

        static async Task<Message> Usage(ITelegramBotClient botClient, Message message,
            CancellationToken cancellationToken)
        {
            const string usage = "Hi! I'm Simple Crud Boy! You can use the following commands:\n" +
                                 "/fill_data       - Put data to database from user.json file\n" +
                                 "/get_persons     - Get all persons from database\n" +
                                 "/delete_person   - Delete person by Id\n" +
                                 "/delete_all      - Delete all persons from database\n" +
                                 "/get_weather     - Get current weather";

            return await botClient.SendTextMessageAsync(
                message.Chat.Id,
                usage,
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
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