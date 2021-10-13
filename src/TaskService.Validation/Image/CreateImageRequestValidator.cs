using System.Collections.Generic;
using FluentValidation;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Image.Interfaces;

namespace LT.DigitalOffice.TaskService.Validation.Image
{
  public class CreateImageRequestValidator : AbstractValidator<CreateImageRequest>, ICreateImageRequestValidator
  {
    public CreateImageRequestValidator(
      ITaskRepository taskRepository,
      IImageValidator imageValidator)
    {
      List<string> errors = new();

      RuleFor(request => request.TaskId)
        .Must(id => taskRepository.DoesExist(id))
        .WithMessage("Task must exist.");

      RuleFor(images => images)
        .NotNull().WithMessage("List must not be null.")
        .NotEmpty().WithMessage("List must not be empty.");

      RuleFor(images => images.TaskId)
        .NotEmpty().WithMessage("Image's Id must not be empty.");

      RuleForEach(images => images.Images)
        .SetValidator(imageValidator)
        .WithMessage("Incorrect image.");
    }
  }
}
