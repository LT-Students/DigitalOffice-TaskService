using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces
{
  [AutoInject]
  public interface IEditTaskPropertyRequestValidator : IValidator<JsonPatchDocument<EditTaskPropertyRequest>>
  {
  }
}
