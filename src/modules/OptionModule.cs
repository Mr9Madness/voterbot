using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using VoterBot.Models;

[Group("option")]
public class OptionModule : ModuleBase<SocketCommandContext>
{
    public VoterBot.Models.VoterContext VoterContext { get; set; }
    public DiscordSocketClient SocketClient { get; set; }

    [Command("add"), Summary("Adds an option to the list")]
    public async Task AddAsync([Remainder] [Summary("Entry to add to the list")] string entry)
    {
        var voteCount = VoterContext.Votes.Where(f => f.GuildId == Context.Guild.Id);
        if (voteCount.Count(f => f.UserId == Context.User.Id) > 1)
            throw new Exception("You already have 2 or more votes");

        var vote = new VoterBot.Models.Votes
        {
            Id = Guid.NewGuid(),
            Name = GetNameFromLink(new Uri(entry)),
            VoteUrl = entry,
            UserId = Context.User.Id,
            GuildId = Context.Guild.Id,
        };
        vote.MessageId = await WriteOption(vote);

        await VoterContext.AddAsync(vote);
        await VoterContext.SaveChangesAsync();

        await ReplyAsync("Option added.");
    }

    [Command("remove"), Summary("Removes an option the user created with the specified id or the last one if left empty")]
    public async Task RemoveAsync([Remainder] string optionId = null)
    {
        VoterBot.Models.Votes vote = null;
        if (string.IsNullOrWhiteSpace(optionId) || Guid.Parse(optionId) == default(Guid))
            vote = VoterContext.Votes.Last(v => v.UserId == Context.User.Id);
        else
        {
            vote = VoterContext.Votes.Find(Guid.Parse(optionId));
            if (vote.UserId != Context.User.Id)
            {
                await ReplyAsync("This is not a option you added.");
                return;
            }
        }
        VoterContext.Votes.Remove(vote);
        await VoterContext.SaveChangesAsync();

        await ReplyAsync("Option removed.");
    }

    [Command("adminremove"), Summary("Removes an option with the specified id (only for admins)"), RequireUserPermission(Discord.GuildPermission.Administrator)]
    public async Task RemoveAdminAsync([Remainder] string optionId)
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

    public string VotesToString(IEnumerable<VoterBot.Models.Votes> enumerable)
    {
        string value = "";
        foreach (VoterBot.Models.Votes vote in enumerable)
            value += VoteToString(vote);
        return value;
    }

    private string GetNameFromLink(Uri entry) => (entry.Host) switch
    {
        "myanimelist.net" => entry.Segments[^1].Replace('_', ' '),
        _ => "No name can be found"
    };

    public async Task<ulong> WriteOption(VoterBot.Models.Votes vote)
    {
        var channel = VoterContext.GuildChannel.FirstOrDefault(g => g.GuildId == Context.Guild.Id);
        if (channel == null) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

        var guildChannel = SocketClient.GetGuild(channel.GuildId).GetTextChannel(channel.ChannelId);
        if (guildChannel == null) throw new Exception("No guild set. Please use '!votes channel set <channel>'");

        IUserMessage message = await guildChannel.SendMessageAsync(VoteToString(vote));
        await message.AddReactionAsync(new Emoji("\u2B06"));
        await message.AddReactionAsync(new Emoji("\u2B07"));

        return message.Id;
    }

    public string VoteToString(VoterBot.Models.Votes vote) => $"{vote.Name}\r\n{vote.VoteUrl}\r\n\r\n";
}