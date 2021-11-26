using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Validation.Task.Interfaces
{
  [AutoInject]
  public interface ICreateTaskRequestValidator : IValidator<CreateTaskRequest>
  {
  }
}
