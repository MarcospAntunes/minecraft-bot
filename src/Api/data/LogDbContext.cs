using Microsoft.EntityFrameworkCore;
using Models;


namespace Data
{
  public class LogDb : DbContext
  {
    public LogDb(DbContextOptions<LogDb> options) : base(options) { }
    public DbSet<Log> Logs => Set<Log>();
  }
}
