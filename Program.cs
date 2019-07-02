using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace VoterBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            using (ServiceProvider services = ConfigureServices())
            {
                using (DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>())
                {
                    await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
                    await client.StartAsync();
                    client.Ready += () =>
                    {
                        Console.WriteLine("Bot is connected!");
                        return Task.CompletedTask;
                    };
                }

                await services.GetRequiredService<Services.VoterCommands>().InitializeAsync();
            }

            await Task.Delay(-1);
        }

        private ServiceProvider ConfigureServices() => new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<Services.VoterDatabaseService>()
            .AddSingleton<Services.VoterCommands>()
            .BuildServiceProvider();
    }
}
