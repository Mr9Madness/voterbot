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
        public async Task List( [Remainder] [Summary("top {number} of entries to get")] string entriesAmount = "top3" )
        {
            if( !entriesAmount.StartsWith("top") )
            {
                await ReplyAsync("Command doesn't include a top {number}");
                return;
            }
            if( !VoterContext.Votes.Any(v => v.GuildId == Context.Guild.Id) )
            {
                await ReplyAsync("No votes");
                return;
            }
            int amount = int.Parse( string.Join("", entriesAmount.Split(' ')).Substring(3, 1) );
            SocketTextChannel guildChannel = GetOutputGuildChannel().Channel;

            ICollection<(string name, int up, int down, int total)> values = new List<(string name, int up, int down, int total)>();

            foreach( Votes v in VoterContext.Votes.Take(amount).Where(v => v.GuildId == Context.Guild.Id) )
            {
                if( v.MessageId == default ) return;
                IUserMessage message = await guildChannel.GetMessageAsync(v.MessageId) as IUserMessage;

                int up = message.Reactions[new Emoji("\u2B06")].ReactionCount - 1;
                int down = message.Reactions[new Emoji("\u2B07")].ReactionCount - 1;

                values.Add((v.Name, up, down, up - down));
            }

            string content = "";
            int num = 1;
            foreach( (string name, int up, int down, int total) in values.OrderByDescending(v => v.total) )
                content += $"{ordinal(num++)} - {name}: Total {total} - Up {up} Down {down}\r\n";

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

        private string ordinal(int number)
        {
            var ones = number % 10;
            var tens = Math.Floor(number / 10f) % 10;
            if (tens == 1) return number + "th";

            switch (ones)
            {
                case 1: return number + "st";
                case 2: return number + "nd";
                case 3: return number + "rd";
                default: return number + "th";
            }
        }
    }
}