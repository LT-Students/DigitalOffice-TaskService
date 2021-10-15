using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Validators;
using LT.DigitalOffice.Kernel.Validators;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Task.Interfaces;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TaskService.Validation.Task
{
  public class EditTaskRequestValidator : BaseEditRequestValidator<EditTaskRequest>, IEditTaskRequestValidator
  {
    private readonly ITaskPropertyRepository _taskPropertyRepository;

    private void HandleInternalPropertyValidation(Operation<EditTaskRequest> requestedOperation, CustomContext context)
    {

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
        x => x == OperationType.Replace,
        new()
        {
          { x => !string.IsNullOrEmpty(x.value?.ToString().Trim()), "Name not must be null or empty." },
          { x => x.value.ToString()?.Trim()?.Length < 150, "Name is too long" }
        });

      #endregion

      #region description

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.Description),
        x => x == OperationType.Replace,
        new()
        {
          {
            x => x.value?.ToString().Trim().Length < 150,
            "Description is too long"
          }
        });

      #endregion

      #region assignedto

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.AssignedTo),
        x => x == OperationType.Replace,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of AssignedTo." }
        });

      #endregion

      #region priorityid

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.PriorityId),
        x => x == OperationType.Replace,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of PriorityId." }
        });

      AddFailureForPropertyIfAsync(
        nameof(EditTaskRequest.PriorityId),
        x => x == OperationType.Replace,
        new()
        {
          { async x => await _taskPropertyRepository.DoesExistAsync(Guid.Parse(x.value.ToString()), TaskPropertyType.Priority), "The priority must exist." }
        });

      #endregion

      #region statusid

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.StatusId),
        x => x == OperationType.Replace,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of StatusId." }
        });

      AddFailureForPropertyIfAsync(
        nameof(EditTaskRequest.StatusId),
        x => x == OperationType.Replace,
        new()
        {
          { async x => await _taskPropertyRepository.DoesExistAsync(Guid.Parse(x.value.ToString()), TaskPropertyType.Status), "The status must exist." }
        });

      #endregion

      #region typeid

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.TypeId),
        x => x == OperationType.Replace,
        new()
        {
          { x => Guid.TryParse(x.value.ToString(), out var _), "Incorrect format of TypeId." }
        });

      AddFailureForPropertyIfAsync(
        nameof(EditTaskRequest.TypeId),
        x => x == OperationType.Replace,
        new()
        {
          { async x => await _taskPropertyRepository.DoesExistAsync(Guid.Parse(x.value.ToString()), TaskPropertyType.Type), "The type must exist." }
        });

      #endregion

      #region PlannedMinutes

      AddFailureForPropertyIf(
        nameof(EditTaskRequest.PlannedMinutes),
        x => x == OperationType.Replace,
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
