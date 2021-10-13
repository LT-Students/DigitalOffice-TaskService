using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.ProjectService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Validation.Image.Interfaces
{
  [AutoInject]
  public interface IImageValidator : IValidator<ImageContent>
  {
  }
}
