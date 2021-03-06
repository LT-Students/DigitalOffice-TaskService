using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.PatchDocument.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Task.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task
{
  public class EditTaskCommand : IEditTaskCommand
  {
    private readonly ITaskRepository _taskRepository;
    private readonly IEditTaskRequestValidator _validator;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPatchDbTaskMapper _mapper;
    private readonly ILogger<EditTaskCommand> _logger;
    private readonly IRequestClient<ICheckProjectUsersExistenceRequest> _rcCheckProjectUsers;
    private readonly IResponseCreator _responseCreator;

    private async Task<bool> DoesProjectUserExistAsync(Guid projectId, Guid userId)
    {
      var logMessage = "Cannot check project users existence.";

      try
      {
        var response = await _rcCheckProjectUsers.GetResponse<IOperationResult<List<Guid>>>(
          ICheckProjectUsersExistenceRequest.CreateObj(projectId, new() { userId }));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body?.Any() ?? false;
        }

        _logger.LogWarning(logMessage);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      return false;
    }

    private async Task<bool> ValidateAsync(DbTask task, JsonPatchDocument<EditTaskRequest> patch, List<string> errors)
    {
      var newAssignedTo = patch.Operations.FirstOrDefault(
        o => o.path[1..].Equals(nameof(EditTaskRequest.AssignedTo), StringComparison.OrdinalIgnoreCase));

      if (newAssignedTo == null
        || await DoesProjectUserExistAsync(task.ProjectId, Guid.Parse(newAssignedTo.value.ToString())))
      {
        return true;
      }

      errors.Add("User must be in project.");

      return false;
    }

    public EditTaskCommand(
      ITaskRepository taskRepository,
      IEditTaskRequestValidator validator,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IPatchDbTaskMapper mapper,
      ILogger<EditTaskCommand> logger,
      IRequestClient<ICheckProjectUsersExistenceRequest> rcCheckProjectUsers,
      IResponseCreator responseCreator)
    {
      _taskRepository = taskRepository;
      _validator = validator;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _mapper = mapper;
      _logger = logger;
      _rcCheckProjectUsers = rcCheckProjectUsers;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid taskId, JsonPatchDocument<EditTaskRequest> patch)
    {
      List<string> errors = new();

      DbTask task = await _taskRepository.GetAsync(taskId, false);

      Guid requestUserId = _httpContextAccessor.HttpContext.GetUserId();

      if (!await _accessValidator.IsAdminAsync()
        && !await DoesProjectUserExistAsync(task.ProjectId, requestUserId))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(patch);

      errors.AddRange(validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());

      if (!validationResult.IsValid || !await ValidateAsync(task, patch, errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      await _taskRepository.EditAsync(task, _mapper.Map(patch));

      return new OperationResultResponse<bool>
      {
        Status = errors.Any() ?
          OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess,
        Body = true,
        Errors = errors
      };
    }
  }
}
