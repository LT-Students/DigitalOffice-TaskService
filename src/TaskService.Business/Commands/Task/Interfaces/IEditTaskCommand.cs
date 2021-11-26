using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces
{
  [AutoInject]
  public interface IEditTaskCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid taskId, JsonPatchDocument<EditTaskRequest> patch);
  }
}
