using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace voterbot
{
    public class VoterCommands
    {
        private readonly DiscordSocketClient _sockerClient;
        private readonly CommandService _commandService;

        public VoterCommands(DiscordSocketClient socketClient, CommandService command)
        {
            _sockerClient = socketClient;
            _commandService = command;
        }

        public async Task InstallCommandsAsync()
        {
            _sockerClient.MessageReceived += HandleCommandAsync;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_sockerClient.CurrentUser, ref argPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(_sockerClient, message);

            var result = await _commandService.ExecuteAsync(context, argPos, null);
        }
    }
}
