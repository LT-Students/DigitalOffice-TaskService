using FluentValidation;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Validation.Image.Interfaces
{
  [AutoInject]
  public interface ICreateImageRequestValidator : IValidator<CreateImageRequest>
  {
  }
}
