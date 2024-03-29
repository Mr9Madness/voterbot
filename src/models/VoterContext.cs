using Microsoft.EntityFrameworkCore;

namespace VoterBot.Models
{
    public class VoterContext : DbContext
    {
        public DbSet<Votes> Votes { get; set; }
        public DbSet<GuildChannel> GuildChannel { get; set; }

        protected override void OnConfiguring( DbContextOptionsBuilder optionsBuilder )
        {
            optionsBuilder.UseSqlite("Data Source=votes.db");
        }
    }
}