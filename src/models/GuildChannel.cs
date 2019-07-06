using System.ComponentModel.DataAnnotations;

namespace VoterBot.Models
{
    public class GuildChannel
    {
        [Key]
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}