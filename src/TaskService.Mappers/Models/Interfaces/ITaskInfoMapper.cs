using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface ITaskInfoMapper
  {
    TaskInfo Map(DbTask dbTask, ProjectData project);
  }
}
