using JarvisBot.Background;
using JarvisBot.Exchange.AlfaBankInSyncRates;
using JarvisBot.SecureService;
using JarvisBot.Weather;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JarvisBot
{
    class JarvisMind
    {

        private static ILogger _logger = LogManager.GetCurrentClassLogger();

        public JarvisClientSettings JarvisClientSettings { get; set; }
        public EncryptionSettings EncryptionSettings { get; set; }


        private static async Task Main(string[] args)
        {
            _logger.Info("Begine start Jarvis");

            using (Mutex mutex = new Mutex(true, "MyApp", out bool isNewInstance))
            {
                using var tocen = new CancellationTokenSource();

                if (!isNewInstance)
                {
                    _logger.Info("Попытка запустить второй экземпляр программы !!!");
                    Console.WriteLine("Попытка запустить второй экземпляр программы !!!");
                    return;
                }
                var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;
                    _logger.Info($"App start with [{env.EnvironmentName}] environment");

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

                    // add UserSecrets when Development
                    if (env.IsDevelopment())
                    {
                        config.AddUserSecrets<JarvisMind>();
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    var env = context.HostingEnvironment;

                    services.AddSingleton<CommunicationMethods>();
                    services.AddSingleton<JarvisBackgroundService>();
                    services.AddSingleton<ExchangeRateLoder>();
                    services.AddSingleton<OldExchangeRates>();
                    services.AddSingleton<WeatherLoder>();
                    services.AddSingleton<EncryptionHelper>();

                    services.Configure<JarvisClientSettings>(configuration.GetSection(nameof(JarvisClientSettings)));
                    services.Configure<EncryptionSettings>(configuration.GetSection(nameof(EncryptionSettings)));

                    var provider = services.BuildServiceProvider();
                    var encryption = provider.GetRequiredService<EncryptionHelper>();
                    var settins = provider.GetRequiredService<IOptions<JarvisClientSettings>>().Value;
                    var encryptionSettings = provider.GetRequiredService<IOptions<EncryptionSettings>>().Value;

                    JarvisClientSettings clientSettings;
                    if (env.IsProduction())
                    {
                        var idClient = encryption.Decrypt(settins.AdminChatIdString);
                        if (!int.TryParse(idClient, out int adminChatId))
                        {
                            _logger.Error("AdminChatId расшифрован некорректно и не является числом.");
                            throw new InvalidOperationException("AdminChatId must be a valid integer after decryption.");
                        }                        

                        clientSettings = new JarvisClientSettings
                        {
                            TelegramBotClient = encryption.Decrypt(settins.TelegramBotClient),
                            AdminChatId = adminChatId,
                            YandexKey = encryption.Decrypt(settins.YandexKey),
                            AlfabankRate = encryption.Decrypt(settins.AlfabankRate),
                            YandexWeather = encryption.Decrypt(settins.YandexWeather),
                            OldExchangeRatesPath = encryption.Decrypt(settins.OldExchangeRatesPath)
                        };
                    }
                    else
                    {
                        clientSettings = settins;
                    }

                    services.AddSingleton(clientSettings);
                    services.AddHostedService<JarvisBackgroundService>();
                })
                .UseWindowsService()
                .Build();
                try
                {
                    _logger.Info("Begine RunAsync");

                    await host.RunAsync();
                }
                catch (Exception exeption)
                {
                    _logger.Error($"Jarvis don`t started{exeption}");
                    throw;
                }
            }
        }
    }
}