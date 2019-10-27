using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoterBot.Models;

[Group("option")]
public class OptionModule : ModuleBase<SocketCommandContext>
{
    public VoterContext VoterContext { get; set; }
    public DiscordSocketClient SocketClient { get; set; }

    [Command("add"), Summary("Adds an option to the list")]
    public async Task AddAsync( [Remainder] [Summary("Entry to add to the list")] string entry )
    {
        var voteCount = VoterContext.Votes.Where(f => f.GuildId == Context.Guild.Id);
        if( voteCount.Count(f => f.UserId == Context.User.Id) > 1 )
            throw new Exception("You already have 2 votes");

        var vote = new Votes
        {
            Id = Guid.NewGuid(),
            Name = GetNameFromLink(new Uri(entry)),
            VoteUrl = entry,
            UserId = Context.User.Id,
            GuildId = Context.Guild.Id,
            VoteDate = DateTime.UtcNow,
        };
        vote.MessageId = await WriteOption(vote);

        try
        {
            await VoterContext.AddAsync(vote);
            await VoterContext.SaveChangesAsync();
        }
        catch( Exception ex )
        {
            throw new Exception(ex.InnerException.Message, ex.InnerException.InnerException);
        }

        await ReplyAsync("Option added.");
    }

    [Command("remove"), Summary("Removes an option the user created with the specified id or the last one if left empty")]
    public async Task RemoveAsync( [Remainder] string optionId = null )
    {
        Votes vote = null;
        if( string.IsNullOrWhiteSpace(optionId) || Guid.Parse(optionId) == default )
            vote = VoterContext.Votes.OrderByDescending(v => v.VoteDate).FirstOrDefault(v => v.UserId == Context.User.Id);
        else
        {
            vote = VoterContext.Votes.Find(Guid.Parse(optionId));
            if( vote.UserId != Context.User.Id )
            {
                await ReplyAsync("This is not a option you added.");
                return;
            }
        }
        if( vote == null )
        {
            await ReplyAsync("You have no options");
            return;
        }

        VoterContext.Votes.Remove(vote);
        await VoterContext.SaveChangesAsync();

        await RemoveOption(vote);

        await ReplyAsync($"Option {vote.Name} removed.");
    }

    [Command("adminremoveall"), Summary("Removes all options (only for admins)"), RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task RemoveAllAsAdminAsync( [Remainder] string optionId )
    {
        VoterContext.Votes.RemoveRange(VoterContext.Votes.Where(o => o.GuildId == Context.Guild.Id));
        await VoterContext.SaveChangesAsync();

        await ReplyAsync("Option removed.");
    }

    [Command("adminremove"), Summary("Removes an option with the specified id (only for admins)"), RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task RemoveAsAdminAsync( [Remainder] string optionId )
    {
        VoterContext.Votes.Remove(VoterContext.Votes.Find(Guid.Parse(optionId)));
        await VoterContext.SaveChangesAsync();

        await ReplyAsync("Option removed.");
    }

    [Command("list"), Summary("Lists the option list")]
    public async Task List()
    {
        await ReplyAsync(VotesToString(VoterContext.Votes.Where(s => s.GuildId == Context.Guild.Id)));
    }

    public string VotesToString( IEnumerable<Votes> enumerable )
    {
        string value = "";
        foreach( Votes vote in enumerable )
            value += VoteToStringAsync(vote);
        return value;
    }

    private string GetNameFromLink( Uri entry ) => entry.Host switch
    {
        "myanimelist.net" => entry.Segments[^1].Replace('_', ' '),
        _ => "No name can be found",
    };

    public GuildChannel GetOutputChannel()
    {
        GuildChannel guildChannel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
        if( guildChannel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

        guildChannel.Channel = SocketClient.GetGuild(guildChannel.GuildId).GetTextChannel(guildChannel.ChannelId);
        if( guildChannel.Channel == null ) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

        return guildChannel;
    }

    public async Task<ulong> WriteOption( Votes vote )
    {
        IUserMessage message = await GetOutputChannel().Channel.SendMessageAsync(await VoteToStringAsync(vote));
        await message.AddReactionAsync(new Emoji("\u2B06"));
        await message.AddReactionAsync(new Emoji("\u2B07"));

        return message.Id;
    }

    public async Task RemoveOption( Votes vote ) => await GetOutputChannel().Channel.DeleteMessageAsync(vote.MessageId);

    public async Task<string> VoteToStringAsync( Votes vote ) => $"{vote.Name}\r\nBy {( await Context.Channel.GetUserAsync(vote.UserId) ).Username} On {vote.VoteDate.ToShortTimeString()} {vote.VoteDate.ToShortDateString()} UTC\r\n{vote.VoteUrl}";
}