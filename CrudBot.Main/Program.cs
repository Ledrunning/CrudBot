using CrudBot.DAL.Contracts;
using CrudBot.DAL.Repository;
using CrudBot.Main.Abstraction;
using CrudBot.Main.Configuration;
using CrudBot.Main.Service;
using CrudBot.Weather.Contract;
using CrudBot.Weather.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using Telegram.Bot;

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

    builder.Services.Configure<BotConfiguration>(
        builder.Configuration.GetSection(BotConfiguration.Configuration));
    builder.Services.Configure<DataBaseConfiguration>(
        builder.Configuration.GetSection(DataBaseConfiguration.Configuration));
    builder.Services.Configure<OpenWeatherApi>(
        builder.Configuration.GetSection(OpenWeatherApi.Configuration));

    var connectionString = builder.Configuration
        .GetSection(DataBaseConfiguration.Configuration)
        .Get<DataBaseConfiguration>()!.ConnectionString;

    var botConfig = builder.Configuration
        .GetSection(BotConfiguration.Configuration)
        .Get<BotConfiguration>()!;

    var weatherConfig = builder.Configuration
        .GetSection(OpenWeatherApi.Configuration)
        .Get<OpenWeatherApi>()!;

    builder.Services.AddHttpClient("t.me/my_crud_bot")
        .AddTypedClient<ITelegramBotClient>(httpClient =>
        {
            TelegramBotClientOptions options = new(botConfig.BotToken);
            return new TelegramBotClient(options, httpClient);
        });

    try
    {
        var userRepository = new UserRepository(connectionString);
        await userRepository.CreateTable(CancellationToken.None);

        builder.Services.AddSingleton<IUserRepository>(userRepository);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }

    builder.Services.AddScoped<UpdateHandler>();
    builder.Services.AddScoped<ReceiverService>();
    builder.Services.AddHostedService<PollingService>();
    builder.Services.AddScoped<IUserService, UserService>();

    var geoWebService = new OpenWeatherRestGeoService(
        weatherConfig.ApiKey,
        weatherConfig.BaseGeoUrl,
        weatherConfig.GeoCityLimit,
        weatherConfig.TimeOut);
    builder.Services.AddSingleton<IOpenWeatherGeoRestService>(geoWebService);

    var openWeatherService = new OpenWeatherRestService(
        geoWebService,
        weatherConfig.ApiKey,
        weatherConfig.BaseUrl,
        weatherConfig.TimeOut);
    builder.Services.AddSingleton<IOpenWeatherRestService>(openWeatherService);

    var cultureInfo = new CultureInfo("en-EN");
    CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
    CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception e)
{
    throw new ApplicationException(e.Message);
}