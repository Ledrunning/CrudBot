using CrudBot.DAL.Contracts;
using CrudBot.DAL.Repository;
using CrudBot.Main;
using CrudBot.Main.Abstraction;
using CrudBot.Main.Configuration;
using CrudBot.Main.Service;
using CrudBot.Weather.Contract;
using CrudBot.Weather.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using CrudBot.DAL.Entity;
using Telegram.Bot;

try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // Register configuration
            services.Configure<BotConfiguration>(
                context.Configuration.GetSection(BotConfiguration.Configuration));
            services.Configure<DataBaseConfiguration>(
                context.Configuration.GetSection(DataBaseConfiguration.Configuration));
            services.Configure<OpenWeatherApi>(
                context.Configuration.GetSection(OpenWeatherApi.Configuration));

            var provider = services.BuildServiceProvider();

            var connectionString = provider.GetConfiguration<DataBaseConfiguration>().ConnectionString;
            var botConfig = provider.GetConfiguration<BotConfiguration>();
            var apiKey = provider.GetConfiguration<OpenWeatherApi>().ApiKey;
            var baseUrl = provider.GetConfiguration<OpenWeatherApi>().BaseUrl;
            var timeOut = provider.GetConfiguration<OpenWeatherApi>().TimeOut;
            var baseGeoUrl= provider.GetConfiguration<OpenWeatherApi>().BaseGeoUrl;
            var cityLimit = provider.GetConfiguration<OpenWeatherApi>().GeoCityLimit;

            // Register named HttpClient to benefits from IHttpClientFactory
            // and consume it with ITelegramBotClient typed client.
            // More read:
            //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
            //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient("t.me/my_crud_bot")
                .AddTypedClient<ITelegramBotClient>(httpClient =>
                {
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

            var userRepository = new UserRepository(connectionString);
            userRepository.CreateTable(CancellationToken.None).GetAwaiter();

            services.AddSingleton<IUserRepository>(userRepository);
            services.AddScoped<UpdateHandler>();
            services.AddScoped<ReceiverService>();
            services.AddHostedService<PollingService>();
            services.AddScoped<IUserService, UserService>();

            var geoWebService = new OpenWeatherRestGeoService(apiKey, baseGeoUrl, cityLimit, timeOut);
            services.AddSingleton<IOpenWeatherGeoRestService>(geoWebService);

            var openWeatherService = new OpenWeatherRestService(geoWebService, apiKey, baseUrl, timeOut);
            services.AddSingleton<IOpenWeatherRestService>(openWeatherService);
        })
        .Build();

        var cultureInfo = new CultureInfo("en-EN");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

    await host.RunAsync();
}
catch (Exception e)
{
    throw new ApplicationException(e.Message);
}