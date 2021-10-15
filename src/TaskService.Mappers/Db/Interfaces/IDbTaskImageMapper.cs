using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TaskService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbTaskImageMapper
  {
    DbTaskImage Map(CreateImageRequest request, Guid imageId);

    DbTaskImage Map(Guid taskId, Guid imageId);
  }
}
