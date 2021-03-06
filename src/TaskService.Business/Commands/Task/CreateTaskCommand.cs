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
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Image;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.ProjectService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Task.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task
{
  public class CreateTaskCommand : ICreateTaskCommand
  {
    private readonly ITaskRepository _repository;
    private readonly IAccessValidator _accessValidator;
    private readonly ICreateTaskRequestValidator _validator;
    private readonly IDbTaskMapper _mapperTask;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CreateTaskCommand> _logger;
    private readonly IResponseCreator _responseCreator;
    private readonly IRequestClient<ICreateImagesRequest> _rcImages;
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

    private async Task<List<Guid>> CreateImageAsync(List<ImageContent> projectImages, Guid userId, List<string> errors)
    {
      if (projectImages == null || !projectImages.Any())
      {
        return null;
      }

      var logMessage = "Errors while creating images. Errors: {Errors}";

      try
      {
        var response =
          await _rcImages.GetResponse<IOperationResult<ICreateImagesResponse>>(
            ICreateImagesRequest.CreateObj(
              projectImages.Select(x => new CreateImageData(x.Name, x.Content, x.Extension)).ToList(),
              ImageSource.Project,
              _httpContextAccessor.HttpContext.GetUserId()));

        if (response.Message.IsSuccess && response.Message.Body.ImagesIds != null)
        {
          return response.Message.Body.ImagesIds;
        }

        _logger.LogWarning(
          logMessage,
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      errors.Add("Can not create images. Please try again later.");

      return null;
    }

    public CreateTaskCommand(
      ITaskRepository repository,
      ICreateTaskRequestValidator validator,
      IDbTaskMapper mapperTask,
      IHttpContextAccessor httpContextAccessor,
      IAccessValidator accessValidator,
      ILogger<CreateTaskCommand> logger,
      IRequestClient<ICreateImagesRequest> rcImages,
      IRequestClient<ICheckProjectUsersExistenceRequest> rcCheckProjectUsers,
      IResponseCreator responseCreator)
    {
      _repository = repository;
      _validator = validator;
      _mapperTask = mapperTask;
      _httpContextAccessor = httpContextAccessor;
      _rcImages = rcImages;
      _rcCheckProjectUsers = rcCheckProjectUsers;
      _accessValidator = accessValidator;
      _logger = logger;
      _responseCreator = responseCreator;
    }

    public async Task<OperationResultResponse<Guid>> ExecuteAsync(CreateTaskRequest request)
    {
      var authorId = _httpContextAccessor.HttpContext.GetUserId();
      var errors = new List<string>();

      if (!await _accessValidator.IsAdminAsync()
        && !await DoesProjectUserExistAsync(request.ProjectId, authorId))
      {
        return _responseCreator.CreateFailureResponse<Guid>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      errors.AddRange(validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());

      if (!validationResult.IsValid)
      {
        return _responseCreator.CreateFailureResponse<Guid>(HttpStatusCode.BadRequest, errors);
      }

      OperationResultResponse<Guid> response = new();

      var imagesIds = await CreateImageAsync(request.TaskImages, authorId, response.Errors);

      response.Body = await _repository.CreateAsync(_mapperTask.Map(request, imagesIds));

      response.Status = response.Errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess;

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return response;
    }
  }
}
