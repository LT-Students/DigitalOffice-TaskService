using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface ITaskPropertyInfoMapper
  {
    TaskPropertyInfo Map(DbTaskProperty dbTaskProperty);
  }
}
