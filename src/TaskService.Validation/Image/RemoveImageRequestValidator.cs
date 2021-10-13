using FluentValidation;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Image.Interfaces;

namespace LT.DigitalOffice.TaskService.Validation.Image
{
  public class RemoveImageRequestValidator : AbstractValidator<RemoveImageRequest>, IRemoveImageRequestValidator
  {
    public RemoveImageRequestValidator()
    {
      RuleFor(list => list.ImagesIds)
        .NotNull().WithMessage("List must not be null.")
        .NotEmpty().WithMessage("List must not be empty.")
        .ForEach(x => x.NotEmpty().WithMessage("Image's Id must not be empty."));
    }
  }
}
