using System;

namespace VoterBot.Models
{
    public class Votes
    {
        public Guid Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public string Name { get; set; }
        public string VoteUrl { get; set; }
    }
}