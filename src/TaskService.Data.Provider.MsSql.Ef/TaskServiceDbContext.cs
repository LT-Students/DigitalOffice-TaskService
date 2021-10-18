using System.Reflection;
using System.Threading.Tasks;
using LT.DigitalOffice.TaskService.Data.Provider;
using LT.DigitalOffice.TaskService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TaskService.Data.Provider.MsSql.Ef
{
  /// <summary>
  /// A class that defines the tables and its properties in the database of TaskService.
  /// </summary>
  public class TaskServiceDbContext : DbContext, IDataProvider
  {
    public DbSet<DbTask> Tasks { get; set; }
    public DbSet<DbTaskProperty> TaskProperties { get; set; }
    public DbSet<DbTaskImage> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfigurationsFromAssembly(
        Assembly.Load("LT.DigitalOffice.TaskService.Models.Db"));
    }

    public TaskServiceDbContext(DbContextOptions<TaskServiceDbContext> options)
      : base(options)
    {
    }

    public void Save()
    {
      SaveChanges();
    }

    public object MakeEntityDetached(object obj)
    {
      Entry(obj).State = EntityState.Detached;

      return Entry(obj).State;
    }

    public void EnsureDeleted()
    {
      Database.EnsureDeleted();
    }

    public bool IsInMemory()
    {
      return Database.IsInMemory();
    }

    public async Task SaveAsync()
    {
      await SaveChangesAsync();
    }
  }
}
