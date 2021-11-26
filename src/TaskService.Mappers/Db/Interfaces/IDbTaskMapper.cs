using System;
using System.Collections.Generic;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbTaskMapper
  {
    DbTask Map(CreateTaskRequest taskRequest, List<Guid> imagesIds);
  }
}
