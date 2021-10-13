using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using System;

namespace LT.DigitalOffice.TaskService.Mappers.Db.Interfaces
{
  [AutoInject]
  public interface IDbTaskPropertyMapper
  {
    DbTaskProperty Map(CreateTaskPropertyRequest request);
  }
}
