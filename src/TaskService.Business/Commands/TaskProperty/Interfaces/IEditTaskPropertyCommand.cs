using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces
{
  [AutoInject]
  public interface IEditTaskPropertyCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(Guid taskPropertyId, JsonPatchDocument<EditTaskPropertyRequest> patch);
  }
}
