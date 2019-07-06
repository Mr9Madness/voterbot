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
        static void Main()
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            using ServiceProvider services = ConfigureServices();
            using DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            await client.LoginAsync(TokenType.Bot, "NTg2NzAxMzc2OTM2MjE0NTI4.XR_sDQ.OdOzEhCGjbWVTcZw7FextOmPV8Q");
            await client.StartAsync();
            client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };

            await services.GetRequiredService<Services.VoterCommandService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private ServiceProvider ConfigureServices() => new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddDbContext<Models.VoterContext>()
            .AddSingleton<Services.VoterCommandService>()
            .BuildServiceProvider();
    }
}
