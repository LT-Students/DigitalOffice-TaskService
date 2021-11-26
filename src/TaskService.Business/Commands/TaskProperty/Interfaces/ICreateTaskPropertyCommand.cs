using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using System;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces
{
  [AutoInject]
  public interface ICreateTaskPropertyCommand
  {
    Task<OperationResultResponse<Guid>> ExecuteAsync(CreateTaskPropertyRequest request);
  }
}
