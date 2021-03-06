using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TaskService.Data.Interfaces;
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

    private async Task<bool> DoesProjectExistAsync(Guid projectId)
    {
      var logMessage = "Cannot check project existence.";

      try
      {
        Response<IOperationResult<ICheckProjectsExistence>> response =
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
      ILogger<CreateTaskPropertyRequestValidator> logger,
      ITaskPropertyRepository taskPropertyRepository)
    {
      _rcCheckProjects = rcCheckProjects;
      _logger = logger;

      RuleFor(tp => tp.Name)
        .NotEmpty()
        .MaximumLength(32);

      RuleFor(tp => tp)
        .MustAsync(async (tp, _) => !await taskPropertyRepository.DoesExistNameAsync(tp.ProjectId, tp.Name))
        .WithMessage("Property name must be unique.");

      RuleFor(tp => tp.PropertyType)
        .IsInEnum()
        .WithMessage("Incorrect property type.");

      When(tp => !string.IsNullOrEmpty(tp.Description?.Trim()), () =>
      {
        RuleFor(tp => tp.Description)
          .MaximumLength(300)
          .WithMessage("Task property description is too long.");
      });

      RuleFor(tp => tp.ProjectId)
        .MustAsync(async (id, _) => await DoesProjectExistAsync(id))
        .WithMessage("Project must exist.");
    }
  }
}
