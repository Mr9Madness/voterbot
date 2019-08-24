using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoterBot.Models
{
    public class GuildChannel
    {
        [Key]
        public ulong GuildId { get; set; }

        [NotMapped]
        public Discord.WebSocket.SocketGuild Guild { get; set; }

        public ulong ChannelId { get; set; }

        [NotMapped]
        public Discord.WebSocket.SocketTextChannel Channel { get; set; }
    }
}