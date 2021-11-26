using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Validation.TaskProperty
{
  public class EditTaskPropertyRequestValidator : BaseEditRequestValidator<EditTaskPropertyRequest>, IEditTaskPropertyRequestValidator
  {
    private readonly IRequestClient<ICheckProjectsExistence> _rcCheckProjects;
    private readonly ILogger<EditTaskPropertyRequestValidator> _logger;

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

    private async System.Threading.Tasks.Task HandleInternalPropertyValidationAsync(
      Operation<EditTaskPropertyRequest> requestedOperation,
      CustomContext context)
    {
      Context = context;
      RequestedOperation = requestedOperation;

      #region paths

      AddСorrectPaths(
        new List<string>
        {
          nameof(EditTaskPropertyRequest.Name),
          nameof(EditTaskPropertyRequest.PropertyType),
          nameof(EditTaskPropertyRequest.Description),
          nameof(EditTaskPropertyRequest.ProjectId),
          nameof(EditTaskPropertyRequest.IsActive)
        });

      AddСorrectOperations(nameof(EditTaskPropertyRequest.Name), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskPropertyRequest.PropertyType), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskPropertyRequest.Description), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskPropertyRequest.ProjectId), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskPropertyRequest.IsActive), new List<OperationType> { OperationType.Replace });

      #endregion

      #region name

      AddFailureForPropertyIf(
        nameof(EditTaskPropertyRequest.Name),
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), "Name must not be empty." },
          { x => x.value?.ToString().Trim().Length < 150, "Name is too long." }
        });

      #endregion

      #region description

      AddFailureForPropertyIf(
        nameof(EditTaskPropertyRequest.Description),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => x.value?.ToString().Trim().Length < 150,
            "Description is too long"
          }
        });

      #endregion

      #region PropertyType

      AddFailureForPropertyIf(
        nameof(EditTaskPropertyRequest.PropertyType),
        x => x == OperationType.Replace,
        new()
        {
          { x => Enum.IsDefined(typeof(TaskPropertyType), x.value?.ToString()), "This PropertyType does not exist." }
        });

      #endregion

      #region ProjectId

      await AddFailureForPropertyIfAsync(
        nameof(EditTaskPropertyRequest.ProjectId),
        x => x == OperationType.Replace,
        new()
        {
          { async (x) => Guid.TryParse(x.value.ToString(), out var result) && await DoesProjectExistAsync(result), "Incorrect project id." }
        });

      #endregion

      #region IsActive

      AddFailureForPropertyIf(
        nameof(EditTaskPropertyRequest.IsActive),
        x => x == OperationType.Replace,
        new()
        {
          { x => bool.TryParse(x.value?.ToString(), out var result), "Incorrect taskProperty is active format." }
        });

      #endregion
    }

    public EditTaskPropertyRequestValidator(
      IRequestClient<ICheckProjectsExistence> rcCheckProjects,
      ILogger<EditTaskPropertyRequestValidator> logger)
    {
      _rcCheckProjects = rcCheckProjects;
      _logger = logger;

      RuleForEach(x => x.Operations)
        .CustomAsync(async (x, context, token) => await HandleInternalPropertyValidationAsync(x, context));
    }
  }
}
