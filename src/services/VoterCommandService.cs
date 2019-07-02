using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace VoterBot.Services
{
    public class VoterCommands
    {
        private readonly DiscordSocketClient _sockerClient;
        private readonly CommandService _commands;

        public VoterCommands(ServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _sockerClient = services.GetRequiredService<DiscordSocketClient>();

            _sockerClient.MessageReceived += HandleMessageAsync;
            _commands.CommandExecuted += HandleCommandAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;

            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_sockerClient.CurrentUser, ref argPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(_sockerClient, message);

            await _commands.ExecuteAsync(context, argPos, null);
        }

        private async Task HandleCommandAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if( !command.IsSpecified ) return;
            if( result.IsSuccess ) return;

            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}
