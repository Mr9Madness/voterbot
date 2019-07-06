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

[Group("option")]
public class OptionModule : ModuleBase<SocketCommandContext>
{
    public VoterBot.Models.VoterContext VoterContext { get; set; }
    public DiscordSocketClient SocketClient { get; set; }

    [Command("add")]
    [Summary("Adds a option to the list")]
    public async Task Option( [Remainder] [Summary("Entry to add to the list")] string entry )
    {
        var vote = new VoterBot.Models.Votes
        {
            Id = Guid.NewGuid(),
            Name = GetNameFromLink(entry),
            VoteUrl = entry,
            UserId = Context.User.Id,
            GuildId = Context.Guild.Id,
        };

        var voteCount = VoterContext.Votes.Where(f => f.GuildId == Context.Guild.Id);
        if( voteCount.Count(f => f.UserId == Context.User.Id) > 1 )
            throw new Exception("You already have 2 or more votes");

        await WriteOption(vote);
        await VoterContext.AddAsync(vote);
        await VoterContext.SaveChangesAsync();

        await Context.Channel.SendMessageAsync($"Entry added.");
    }

    private string GetNameFromLink( string entry ) => ( new Uri(entry).Host ) switch
    {
        "myanimelist.net" => new Uri(entry).Segments[^1].Replace('_', ' '),
        _ => "No name can be found"
    };

    [Command("list")]
    [Summary("Lists the option list")]
    public async Task List()
    {
        await Context.Channel.SendMessageAsync(VotesToString(VoterContext.Votes.Where(s => s.GuildId == Context.Guild.Id)));
    }

    public string VotesToString( IQueryable<VoterBot.Models.Votes> enumerable )
    {
        string value = "";
        foreach( VoterBot.Models.Votes vote in enumerable )
            value += VoteToString(vote);
        return value;
    }
    public string VoteToString( VoterBot.Models.Votes vote ) => $"{vote.Name}\r\n{vote.VoteUrl}\r\n\r\n";

    public async Task WriteOption( VoterBot.Models.Votes vote )
    {
        var channel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
        if( channel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

        var guildChannel = SocketClient.GetGuild(channel.GuildId).GetTextChannel(channel.ChannelId);
        if( guildChannel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

        IUserMessage message = await guildChannel.SendMessageAsync(VoteToString(vote));
        await message.AddReactionAsync(new Emoji("\u2B06"));
        await message.AddReactionAsync(new Emoji("\u2B07"));
    }
}