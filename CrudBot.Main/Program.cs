using System.Runtime.InteropServices.ComTypes;
using CrudBot.DAL.Contracts;
using CrudBot.DAL.Repository;
using CrudBot.Main;
using CrudBot.Main.Configuration;
using CrudBot.Main.Model;
using CrudBot.Main.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

var connectionString = "";

try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            // Register Bot configuration
            services.Configure<BotConfiguration>(
                context.Configuration.GetSection(BotConfiguration.Configuration));

            // Register named HttpClient to benefits from IHttpClientFactory
            // and consume it with ITelegramBotClient typed client.
            // More read:
            //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#typed-clients
            //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            services.AddHttpClient("t.me/my_crud_bot")
                .AddTypedClient<ITelegramBotClient>((httpClient, provider) =>
                {
                    var botConfig = provider.GetConfiguration<BotConfiguration>();
                    TelegramBotClientOptions options = new(botConfig.BotToken);
                    connectionString = provider.GetConfiguration<DataBaseConfiguration>().ConnectionString;
                    return new TelegramBotClient(options, httpClient);
                });

            var userRepository = new UserRepository(connectionString);
            userRepository.CreateTable(CancellationToken.None).GetAwaiter();

            services.AddSingleton<IUserRepository>(_ => userRepository);
            services.AddScoped<UpdateHandler>();
            services.AddScoped<ReceiverService>();
            services.AddHostedService<PollingService>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception e)
{
    throw new ApplicationException(e.Message);
}