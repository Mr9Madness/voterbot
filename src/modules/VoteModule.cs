using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VoterBot.Modules
{
    [Group("votes")]
    public class VoteModule : ModuleBase<SocketCommandContext>
    {
        public DiscordSocketClient SocketClient { get; set; }
        public VoterBot.Models.VoterContext VoterContext { get; set; }

        [Command("list"), Summary("Lists the votes with options")]
        public async Task List()
        {
            VoterBot.Models.GuildChannel channel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
            if (channel == null) throw new Exception("No channel set in guild set. Please use '!votes channel set <channel>'");
            SocketTextChannel guildChannel = SocketClient.GetGuild(channel.GuildId).GetTextChannel(channel.ChannelId);
            if (guildChannel == null) throw new Exception("No channel set in guild set. Please use '!votes channel set <channel>'");

            ICollection< ( string name, int up, int down, int total ) > values = new List<(string name, int up, int down, int total)>();

            await VoterContext.Votes.Where(v => v.GuildId == Context.Guild.Id).ForEachAsync( async v => {
                if( v.MessageId == default(ulong) ) return;
                IUserMessage message = await guildChannel.GetMessageAsync( v.MessageId ) as IUserMessage;

                int up = message.Reactions[new Emoji("\u2B06")].ReactionCount;
                int down = message.Reactions[new Emoji("\u2B07")].ReactionCount;

                values.Add( ( v.Name, up, down, up - down ) );
            });

            string content = "";
            foreach (var item in values.OrderBy(v => v.total))
            {
                content += $"{item.name}\r\nTotal: {item.total} Up: {item.up} Down: {item.down}\r\n\r\n";
            }

            await ReplyAsync( content );
        }

        [Command("dataupdate"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
        public async Task Update()
        {
            var channel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
            if( channel == null ) throw new Exception("No channel set in guild set. Please use '!votes channel set <channel>'");
            SocketTextChannel guildChannel = SocketClient.GetGuild(channel.GuildId).GetTextChannel(channel.ChannelId);
            if( guildChannel == null ) throw new Exception("No channel set in guild set. Please use '!votes channel set <channel>'");
            var messages = await guildChannel.GetMessagesAsync().FlattenAsync();
            await VoterContext.Votes.ForEachAsync(v =>
            {
                if (v.MessageId != default(ulong)) return;

                foreach (IMessage item in messages)
                {
                    if (item.Content.Contains(v.Name))
                    {
                        v.MessageId = item.Id;
                        VoterContext.Entry(v).State = EntityState.Modified;
                    }
                }
            });
            await VoterContext.SaveChangesAsync();
            await ReplyAsync("Data updated");
        }

        [Group("channel")]
        public class ChannelModule : VoteModule
        {
            [Command("set"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
            public async Task OutputChannel(IChannel channel)
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
                if (channel == null) throw new Exception("No guild set. Please use '!votes channel set <channel>'");
                var guildChannel = SocketClient.GetChannel(channel.ChannelId);
                await ReplyAsync("#" + guildChannel.ToString());
            }
        }
    }
}