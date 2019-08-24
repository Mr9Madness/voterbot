using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoterBot.Models;

namespace VoterBot.Modules
{
    [Group("votes")]
    public class VoteModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient SocketClient { get; set; }
        public VoterContext VoterContext { get; set; }

        [Command("list"), Summary("Lists the votes with options")]
        public async Task List()
        {
            SocketTextChannel guildChannel = GetOutputGuildChannel().Channel;

            ICollection<(string name, int up, int down, int total)> values = new List<(string name, int up, int down, int total)>();

            foreach( Votes v in VoterContext.Votes.Where(v => v.GuildId == Context.Guild.Id) )
            {
                if( v.MessageId == default ) return;
                IUserMessage message = await guildChannel.GetMessageAsync(v.MessageId) as IUserMessage;

                int up = message.Reactions[new Emoji("\u2B06")].ReactionCount - 1;
                int down = message.Reactions[new Emoji("\u2B07")].ReactionCount - 1;

                values.Add((v.Name, up, down, up - down));
            }

            string content = "";
            foreach( (string name, int up, int down, int total) in values.OrderByDescending(v => v.total) )
            {
                content += $"{name}\r\nTotal: {total} Up: {up} Down: {down}\r\n\r\n";
            }
            if( string.IsNullOrWhiteSpace(content) )
                await ReplyAsync("No votes");
            else
                await ReplyAsync(content);
        }

        [Group("channel")]
        public class ChannelModule : VoteModule
        {
            [Command("set"), RequireContext(ContextType.Guild), RequireUserPermission(GuildPermission.Administrator)] //? change from administrator?
            public async Task OutputChannel( IChannel channel )
            {
                await VoterContext.AddAsync(new GuildChannel
                {
                    ChannelId = channel.Id,
                    GuildId = Context.Guild.Id,
                });
                await VoterContext.SaveChangesAsync();

                await ReplyAsync("Option output channel changed!");
            }

            [Command("get"), RequireContext(ContextType.Guild), RequireUserPermission(GuildPermission.Administrator)] //? change from administrator?
            public async Task GetOutputChannel()
            {
                GuildChannel guildchannel = GetOutputGuildChannel();
                await ReplyAsync("#" + guildchannel.Channel.ToString());
            }

        }
        public GuildChannel GetOutputGuildChannel()
        {
            GuildChannel guildChannel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
            if( guildChannel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

            guildChannel.Channel = SocketClient.GetGuild(guildChannel.GuildId).GetTextChannel(guildChannel.ChannelId);
            if( guildChannel.Channel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

            return guildChannel;
        }

    }
}