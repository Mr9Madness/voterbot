using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace VoterBot
{
    class Program
    {
        static async Task Main()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddUserSecrets<Program>().Build();

            using ServiceProvider services = ConfigureServices();
            using DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            await client.LoginAsync(TokenType.Bot, configuration["BotKey"]);

            await client.StartAsync();
            client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };

            await services.GetRequiredService<Services.VoterCommandService>().InitializeAsync();
            await Task.Delay(-1);
        }

        private static ServiceProvider ConfigureServices() => new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddDbContext<Models.VoterContext>()
            .AddSingleton<Services.VoterCommandService>()
            .BuildServiceProvider();
    }
}
