using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatUser> ChatUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatUser>()
                .HasKey(cu => new { cu.ChatId, cu.UserId });

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.Chat)
                .WithMany(c => c.ChatUsers)
                .HasForeignKey(cu => cu.ChatId);

            modelBuilder.Entity<ChatUser>()
                .HasOne(cu => cu.User)
                .WithMany(u => u.ChatUsers)
                .HasForeignKey(cu => cu.UserId);
        }
    }
}
