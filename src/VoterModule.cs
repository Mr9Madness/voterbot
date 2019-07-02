using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

[Group("votes")]
public class VoteModule: ModuleBase<SocketCommandContext>
{
    public DiscordSocketClient SocketClient { get; set; }
    private IChannel outputChannel { get; set; }

    [Command("channel"), RequireContext(ContextType.Guild), RequireUserPermission(Discord.GuildPermission.Administrator)] //? change from administrator?
    public async Task OutputChannel(IChannel channel)
    {
        outputChannel = channel;
        await ReplyAsync("Option output channel changed!");
    }

    public static async Task AddOption()
    {

    }
}

[Group("option")]
public class OptionModule: ModuleBase<SocketCommandContext>
{
    [Command("add")]
    [Summary("Adds a option to the list")]
    public async Task Option([Remainder] [Summary("Entry to add to the list")] string entry)
    {
        //TODO things
        await Context.Channel.SendMessageAsync($"Entry added.");


    }

}