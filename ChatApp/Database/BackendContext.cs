using Microsoft.EntityFrameworkCore;
using ChatApp.Entites;

namespace ChatApp.Database
{
    public class BackendContext : DbContext
    {
        public BackendContext(DbContextOptions<BackendContext> options) : base(options){ }
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserChat> UsersChats { get; set; }
    }
}
