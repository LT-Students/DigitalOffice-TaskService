using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.PatchDocument.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.ProjectService.Business.Commands.Task
{
  public class EditTaskPropertyCommand : IEditTaskPropertyCommand
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccessValidator _accessValidator;
    private readonly ITaskPropertyRepository _taskPropertyRepository;
    private readonly IPatchDbTaskPropertyMapper _mapper;
    private readonly IEditTaskPropertyRequestValidator _validator;
    private readonly IResponseCreater _responseCreater;

    private async Task<bool> ValidateAsync(Guid? projectId, JsonPatchDocument<EditTaskPropertyRequest> patch, List<string> errors)
    {
      if (!projectId.HasValue)
      {
        errors.Add("Cannot edit default properties.");

        return false;
      }

      Operation<EditTaskPropertyRequest> newName = patch.Operations.FirstOrDefault(o =>
        o.path[1..].Equals(nameof(EditTaskPropertyRequest.Name), StringComparison.OrdinalIgnoreCase));

      if (newName == null)
      {
        return true;
      }

      Operation<EditTaskPropertyRequest> newProjectId = patch.Operations.FirstOrDefault(o =>
        o.path[1..].Equals(nameof(EditTaskPropertyRequest.ProjectId), StringComparison.OrdinalIgnoreCase));

      if (newProjectId == null)
      {
        return !await _taskPropertyRepository.DoesExistNameAsync(projectId.Value, newName.value.ToString());
      }

      return !await _taskPropertyRepository.DoesExistNameAsync(Guid.Parse(newProjectId.value.ToString()), newName.value.ToString());
    }

    public EditTaskPropertyCommand(
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      ITaskPropertyRepository taskPropertyRepository,
      IPatchDbTaskPropertyMapper mapper,
      IEditTaskPropertyRequestValidator validator,
      IResponseCreater responseCreater)
    {
      _httpContextAccessor = httpContextAccessor;
      _accessValidator = accessValidator;
      _taskPropertyRepository = taskPropertyRepository;
      _mapper = mapper;
      _validator = validator;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(Guid taskPropertyId, JsonPatchDocument<EditTaskPropertyRequest> patch)
    {
      DbTaskProperty taskProperty = await _taskPropertyRepository.GetAsync(taskPropertyId);

      if (!await _accessValidator.IsAdminAsync())
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(patch);

      List<string> errors = validationResult.Errors.Select(vf => vf.ErrorMessage).ToList();

      if (!validationResult.IsValid || !await ValidateAsync(taskProperty.ProjectId, patch, errors))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      OperationResultResponse<bool> response = new();

      response.Body = await _taskPropertyRepository.EditAsync(taskProperty, _mapper.Map(patch));

      if (response.Body)
      {
        response.Status = OperationResultStatusType.FullSuccess;
      }
      else
      {
        _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        response.Status = OperationResultStatusType.Failed;
        response.Errors.Add($"Can not edit taskProperty with Id: {taskPropertyId}");
      }

      return response;
    }
  }
}
