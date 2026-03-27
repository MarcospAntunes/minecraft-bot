using Microsoft.EntityFrameworkCore;
using Models;

namespace Data
{
  public class ChatDb : DbContext
  {
    public ChatDb(DbContextOptions<ChatDb> options) : base(options) { }
    public DbSet<Chat> Chats => Set<Chat>();
  }
}