using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace voterbot
{
    class Program
    {
        private readonly DiscordSocketClient _socketClient;
        static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        Program()
        {
            _socketClient = new DiscordSocketClient(new DiscordSocketConfig { MessageCacheSize = 100 });
        }

        public async Task MainAsync()
        {

            await _socketClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DiscordToken"));
            await _socketClient.StartAsync();

            _socketClient.MessageUpdated += MessageUpdated;
            _socketClient.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };


            await Task.Delay(-1);
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {

        }
    }
}
