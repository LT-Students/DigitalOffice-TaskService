using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IProjectInfoMapper
  {
    ProjectInfo Map(ProjectData project);
  }
}
