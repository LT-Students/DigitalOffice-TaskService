using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation.Results;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.PatchDocument.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

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
    private readonly IRequestClient<ICheckProjectUsersExistenceRequest> _rcCheckProjectUsers;
    private readonly ILogger<EditTaskPropertyCommand> _logger;

    private async Task<bool> DoesProjectUserExist(Guid projectId, Guid userId)
    {
      string logMessage = "Cannot check project users existence.";

      try
      {
        Response<IOperationResult<List<Guid>>> response =
          await _rcCheckProjectUsers.GetResponse<IOperationResult<List<Guid>>>(
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
      DbTaskProperty taskProperty = _taskPropertyRepository.Get(taskPropertyId);

      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveProjects)) // TODO rights
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(patch);

      if (!validationResult.IsValid)
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest,
          validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());
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
