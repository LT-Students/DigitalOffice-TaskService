using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Image;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Business.Commands.Image.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Image.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Image
{
  public class RemoveImageCommand : IRemoveImageCommand
  {
    private readonly IImageRepository _imageRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IRequestClient<IRemoveImagesPublish> _rcImages;
    private readonly ILogger<RemoveImageCommand> _logger;
    private readonly IAccessValidator _accessValidator;
    private readonly IRemoveImageRequestValidator _validator;
    private readonly IResponseCreator _responseCreator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRequestClient<ICheckProjectUsersExistenceRequest> _rcCheckProjectUsers;

    private async Task<bool> DoesProjectUserExistAsync(Guid projectId, Guid userId)
    {
      string logMessage = "Cannot check project users existence.";

      try
      {
        Response<IOperationResult<List<Guid>>> response = await _rcCheckProjectUsers.GetResponse<IOperationResult<List<Guid>>>(
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

    private async Task<bool> RemoveImageAsync(List<Guid> ids, List<string> errors)
    {
      if (ids == null || !ids.Any())
      {
        return false;
      }

      var logMessage = "Errors while removing images ids {ids}. Errors: {Errors}";

      try
      {
        Response<IOperationResult<bool>> response =
          await _rcImages.GetResponse<IOperationResult<bool>>(
            IRemoveImagesPublish.CreateObj(ids, ImageSource.Project));

        if (response.Message.IsSuccess)
        {
          return true;
        }

        _logger.LogWarning(
          logMessage,
          string.Join('\n', ids),
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      errors.Add("Can not remove images. Please try again later.");

      return false;
    }

    public RemoveImageCommand(
      IHttpContextAccessor httpContextAccessor,
      IImageRepository imageRepository,
      ITaskRepository taskRepository,
      IRequestClient<IRemoveImagesPublish> rcImages,
      IRequestClient<ICheckProjectUsersExistenceRequest> rcCheckProjectUsers,
      ILogger<RemoveImageCommand> logger,
      IAccessValidator accessValidator,
      IRemoveImageRequestValidator validator,
      IResponseCreator responseCreator)
    {
      _httpContextAccessor = httpContextAccessor;
      _imageRepository = imageRepository;
      _taskRepository = taskRepository;
      _rcImages = rcImages;
      _rcCheckProjectUsers = rcCheckProjectUsers;
      _logger = logger;
      _accessValidator = accessValidator;
      _validator = validator;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(RemoveImageRequest request)
    {
      DbTask task = await _taskRepository.GetAsync(request.TaskId, false);

      if (task == null)
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, new() { "Task must exist." });
      }

      if (!await _accessValidator.IsAdminAsync()
        && !await DoesProjectUserExistAsync(task.ProjectId, _httpContextAccessor.HttpContext.GetUserId()))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(request, out var errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      if (!await RemoveImageAsync(request.ImagesIds, errors))
      {
        return _responseCreator.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        Body = await _imageRepository.RemoveAsync(request.ImagesIds)
      };
    }
  }
}
