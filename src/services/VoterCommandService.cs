using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace VoterBot.Services
{
    public class VoterCommandService
    {
        private readonly DiscordSocketClient _sockerClient;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public VoterCommandService( IServiceProvider services )
        {
            _sockerClient = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _services = services;

            _sockerClient.MessageReceived += HandleMessageAsync;
            _commands.CommandExecuted += HandleCommandAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleMessageAsync( SocketMessage arg )
        {
            if( !( arg is SocketUserMessage message ) ) return;
            int argPos = 0;

            if( !( message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_sockerClient.CurrentUser, ref argPos) ) || message.Author.IsBot ) return;

            var context = new SocketCommandContext(_sockerClient, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private async Task HandleCommandAsync( Optional<CommandInfo> command, ICommandContext context, IResult result )
        {
            if( !command.IsSpecified ) return;
            if( result.IsSuccess ) return;

            await context.Channel.SendMessageAsync($"Error: {result.Error} Reason: {result.ErrorReason}");
        }
    }
}
