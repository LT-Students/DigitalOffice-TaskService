using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Task.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TaskService.Validation.Task
{
  public class EditTaskRequestValidator : AbstractValidator<JsonPatchDocument<EditTaskRequest>>, IEditTaskRequestValidator
  {
    private readonly ITaskPropertyRepository _taskPropertyRepository;

    private void HandleInternalPropertyValidation(Operation<EditTaskRequest> requestedOperation, CustomContext context)
    {
      #region local functions

      void AddСorrectPaths(List<string> paths)
      {
        if (paths.FirstOrDefault(p => p.EndsWith(requestedOperation.path[1..], StringComparison.OrdinalIgnoreCase)) == null)
        {
          context.AddFailure(requestedOperation.path, $"This path {requestedOperation.path} is not available");
        }
      }

      void AddСorrectOperations(
          string propertyName,
          List<OperationType> types)
      {
        if (requestedOperation.path.EndsWith(propertyName, StringComparison.OrdinalIgnoreCase)
            && !types.Contains(requestedOperation.OperationType))
        {
          context.AddFailure(propertyName, $"This operation {requestedOperation.OperationType} is prohibited for {propertyName}");
        }
      }

      void AddFailureForPropertyIf(
          string propertyName,
          Func<OperationType, bool> type,
          Dictionary<Func<Operation<EditTaskRequest>, bool>, string> predicates)
      {
        if (!requestedOperation.path.EndsWith(propertyName, StringComparison.OrdinalIgnoreCase)
            || !type(requestedOperation.OperationType))
        {
          return;
        }

        foreach (var validateDelegate in predicates)
        {
          if (!validateDelegate.Key(requestedOperation))
          {
            context.AddFailure(propertyName, validateDelegate.Value);
          }
        }
      }

      #endregion

      #region paths

      AddСorrectPaths(
        new List<string>
        {
          nameof(EditTaskRequest.Name),
          nameof(EditTaskRequest.Description),
          nameof(EditTaskRequest.AssignedTo),
          nameof(EditTaskRequest.PriorityId),
          nameof(EditTaskRequest.StatusId),
          nameof(EditTaskRequest.TypeId),
          nameof(EditTaskRequest.PlannedMinutes)
        });

      AddСorrectOperations(nameof(EditTaskRequest.Name), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskRequest.Description), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskRequest.AssignedTo), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskRequest.PriorityId), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskRequest.StatusId), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskRequest.TypeId), new List<OperationType> { OperationType.Replace });
      AddСorrectOperations(nameof(EditTaskRequest.PlannedMinutes), new List<OperationType> { OperationType.Replace });

      #endregion

      #region firstname

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.Name),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value.ToString()), "Name is empty." },
          { x => x.value.ToString().Length < 150, "Name is too long" }
        });

      #endregion

      #region description

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.Description),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          {
            x =>
                    {
                      if (string.IsNullOrEmpty(x.ToString()))
                      {
                        return true;
                      }

                      return x.value.ToString().Length < 150;
                    },
            "Name is too long"
          }
        });

      #endregion

      #region assignedto

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.AssignedTo),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of AssignedTo." }
        });

      #endregion

      #region priorityid

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.PriorityId),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of PriorityId." },
          { x => _taskPropertyRepository.DoesExist(Guid.Parse(x.value.ToString()), TaskPropertyType.Priority), "The priority must exist." }
        });

      #endregion

      #region statusid

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.StatusId),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of StatusId." },
          {
            x => _taskPropertyRepository.DoesExist(Guid.Parse(x.value.ToString()),
                    TaskPropertyType.Status),
            "The status must exist."
          }
        });

      #endregion

      #region typeid

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.TypeId),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of TypeId." },
          { x => _taskPropertyRepository.DoesExist(Guid.Parse(x.value.ToString()), TaskPropertyType.Type), "The type must exist." }
        });

      #endregion

      #region PlannedMinutes

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.PlannedMinutes),
        x => x == OperationType.Replace || x == OperationType.Add,
        new()
        {
          { x => int.TryParse(x.value.ToString(), out var minutes) && minutes > 0, "Incorrect format of PlannedMinutes." }
        });

      #endregion
    }

    public EditTaskRequestValidator(
      ITaskPropertyRepository taskPropertyRepository)
    {
      _taskPropertyRepository = taskPropertyRepository;

      RuleForEach(x => x.Operations)
        .Custom(HandleInternalPropertyValidation);
    }
  }
}
