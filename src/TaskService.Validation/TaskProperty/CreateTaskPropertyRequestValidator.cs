using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Validation.TaskProperty
{
  public class CreateTaskPropertyRequestValidator : AbstractValidator<CreateTaskPropertyRequest>, ICreateTaskPropertyRequestValidator
  {
    private readonly IRequestClient<ICheckProjectsExistence> _rcCheckProjects;
    private readonly ILogger<CreateTaskPropertyRequestValidator> _logger;

    private async Task<bool> DoesProjectExist(Guid projectId)
    {
      var logMessage = "Cannot check project existence.";

      try
      {
        var response =
          await _rcCheckProjects.GetResponse<IOperationResult<ICheckProjectsExistence>>(
            ICheckProjectsExistence.CreateObj(new() { projectId }));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ProjectsIds?.Any() ?? false;
        }

        _logger.LogWarning(logMessage);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      return false;
    }

    public CreateTaskPropertyRequestValidator(
      IRequestClient<ICheckProjectsExistence> rcCheckProjects,
      ILogger<CreateTaskPropertyRequestValidator> logger)
    {
      _rcCheckProjects = rcCheckProjects;
      _logger = logger;

      RuleFor(tp => tp.Name)
        .NotEmpty()
        .MaximumLength(32);

      RuleFor(tp => tp.PropertyType)
        .IsInEnum();

      When(tp => !string.IsNullOrEmpty(tp.Description?.Trim()), () =>
      {
        RuleFor(tp => tp.Description)
          .MaximumLength(300)
          .WithMessage("Task property description is too long.");
      });

      RuleFor(tp => tp.ProjectId)
        .MustAsync(async (id, _) => await DoesProjectExist(id))
        .WithMessage("Project must exist.");
    }
  }
}
