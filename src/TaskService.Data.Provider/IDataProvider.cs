using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Database;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.TaskService.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.TaskService.Data.Provider
{
  [AutoInject(InjectType.Scoped)]
  public interface IDataProvider : IBaseDataProvider
  {
    DbSet<DbTask> Tasks { get; set; }
    DbSet<DbTaskProperty> TaskProperties { get; set; }
    DbSet<DbTaskImage> Images { get; set; }
  }
}
