using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Responses;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces
{
  [AutoInject]
  public interface IGetTaskCommand
  {
    Task<OperationResultResponse<TaskResponse>> ExecuteAsync(Guid taskId);
  }
}
