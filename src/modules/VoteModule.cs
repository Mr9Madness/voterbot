using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

[Group("votes")]
public class VoteModule : ModuleBase<SocketCommandContext>
{
    public DiscordSocketClient SocketClient { get; set; }
    public VoterBot.Models.VoterContext VoterContext { get; set; }

    [Command("list"), Summary("Lists the votes with options")]
    public async Task List()
    {

    }

    [Group("channel")]
    public class ChannelModule : VoteModule
    {
        [Command("set"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
        public async Task OutputChannel( IChannel channel )
        {
            await VoterContext.AddAsync(new VoterBot.Models.GuildChannel
            {
                ChannelId = channel.Id,
                GuildId = Context.Guild.Id,
            });
            await VoterContext.SaveChangesAsync();

            await ReplyAsync("Option output channel changed!");
        }

        [Command("get"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
        public async Task GetOutputChannel()
        {
            var channel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
            if( channel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");
            var guildChannel = SocketClient.GetChannel(channel.ChannelId);
            await ReplyAsync("#" + guildChannel.ToString());
        }
    }
}
