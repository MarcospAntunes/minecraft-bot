using Data;
using Models;

public class LogService
{
  private readonly LogDb _db;

  public LogService(LogDb db)
  {
    _db = db;
  }

  public async Task AddMessage(Log log)
  {
    _db.Logs.Add(log);
    await _db.SaveChangesAsync();

  }
}