using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Validation.Task.Interfaces
{
  [AutoInject]
  public interface IEditTaskRequestValidator : IValidator<JsonPatchDocument<EditTaskRequest>>
  {
  }
}
