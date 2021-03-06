using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Mappers.PatchDocument.Interfaces
{
  [AutoInject]
  public interface IPatchDbTaskMapper
  {
    JsonPatchDocument<DbTask> Map(JsonPatchDocument<EditTaskRequest> request);
  }
}
