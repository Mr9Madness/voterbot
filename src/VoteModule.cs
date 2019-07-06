using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

[Group("votes")]
public class VoteModule: ModuleBase<SocketCommandContext>
{
    public DiscordSocketClient SocketClient { get; set; }
    private IChannel outputChannel { get; set; }

    [Group("channel")]
    public class ChannelModule: VoteModule
    {

        [Command("set"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
        public async Task OutputChannel(IChannel channel)
        {
            outputChannel = channel;
            await ReplyAsync("Option output channel changed!");
        }

        [Command("get"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
        public async Task GetOutputChannel()
        {
            await ReplyAsync(outputChannel.Name);
        }

    }



    public static async Task AddOption()
    {

    }
}

[Group("option")]
public class OptionModule: ModuleBase<SocketCommandContext>
{
    public VoterBot.Models.VoterContext VoterContext { get; set; }

    [Command("add")]
    [Summary("Adds a option to the list")]
    public async Task Option([Remainder] [Summary("Entry to add to the list")] string entry)
    {
        await VoterContext.AddAsync<VoterBot.Models.Votes>(new VoterBot.Models.Votes
        {
            Id = Guid.NewGuid(),
            Name = "test",
            VoteUrl = entry,
            UserId = Context.User.Id
        });
        await VoterContext.SaveChangesAsync();
        await Context.Channel.SendMessageAsync($"Entry added.");
    }

    [Command("list")]
    [Summary("Lists the option list")]
    public async Task List()
    {
        await Context.Channel.SendMessageAsync( VoterContext.Votes.First().VoteUrl );
    }

}