using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces
{
  [AutoInject]
  public interface ICreateTaskPropertyRequestValidator : IValidator<CreateTaskPropertyRequest>
  {
  }
}
