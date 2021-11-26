using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using System;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces
{
  [AutoInject]
  public interface ICreateTaskCommand
  {
    Task<OperationResultResponse<Guid>> ExecuteAsync(CreateTaskRequest request);
  }
}
