using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Responses;

namespace LT.DigitalOffice.TaskService.Mappers.Responses.Interfaces
{
  [AutoInject]
  public interface ITaskResponseMapper
  {
    TaskResponse Map(
      DbTask dbTask,
      ProjectData project,
      List<UserData> users,
      List<ImageInfo> taskImages,
      List<ImageInfo> usersImages);
  }
}
