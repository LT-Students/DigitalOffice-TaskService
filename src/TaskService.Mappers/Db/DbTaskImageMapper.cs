using System;
using LT.DigitalOffice.TaskService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Mappers.Db
{
  public class DbTaskImageMapper : IDbTaskImageMapper
  {
    public DbTaskImage Map(CreateImageRequest request, Guid imageId)
    {
      if (request == null)
      {
        return null;
      }

      return new DbTaskImage
      {
        Id = Guid.NewGuid(),
        ImageId = imageId,
        TaskId = request.TaskId
      };
    }

    public DbTaskImage Map(Guid taskId, Guid imageId)
    {
      return new DbTaskImage
      {
        Id = Guid.NewGuid(),
        ImageId = imageId,
        TaskId = taskId
      };
    }
  }
}
