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
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.ProjectService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Business.Commands.Image.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Image.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Image
{
  public class CreateImageCommand : ICreateImageCommand
  {
    private readonly IImageRepository _repository;
    private readonly IRequestClient<ICreateImagesRequest> _rcImages;
    private readonly ILogger<CreateImageCommand> _logger;
    private readonly IAccessValidator _accessValidator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbTaskImageMapper _dbProjectImageMapper;
    private readonly ICreateImageRequestValidator _validator;
    private readonly ITaskRepository _taskRepository;
    private readonly IResponseCreater _responseCreater;

    private async Task<List<Guid>> CreateImagesAsync(List<ImageContent> images, Guid userId, List<string> errors)
    {
      if (images == null || !images.Any())
      {
        return null;
      }

      List<CreateImageData> imagesDatas = images
        .Select(x => new CreateImageData(x.Name, x.Content, x.Extension, userId))
        .ToList();

      string logMessage = "Errors while creating images for task.";

      try
      {
        Response<IOperationResult<ICreateImagesResponse>> response =
          await _rcImages.GetResponse<IOperationResult<ICreateImagesResponse>>(
            ICreateImagesRequest.CreateObj(imagesDatas, ImageSource.Project));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ImagesIds;
        }

        _logger.LogWarning(
          logMessage + "Errors: { Errors }",
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      errors.Add("Can not create images. Please try again later.");

      return null;
    }

    public CreateImageCommand(
      IImageRepository repository,
      IRequestClient<ICreateImagesRequest> rcImages,
      ILogger<CreateImageCommand> logger,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      IDbTaskImageMapper dbProjectImageMapper,
      ICreateImageRequestValidator validator,
      ITaskRepository taskRepository,
      IResponseCreater responseCreater)
    {
      _repository = repository;
      _rcImages = rcImages;
      _logger = logger;
      _accessValidator = accessValidator;
      _httpContextAccessor = httpContextAccessor;
      _dbProjectImageMapper = dbProjectImageMapper;
      _validator = validator;
      _taskRepository = taskRepository;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<List<Guid>>> ExecuteAsync(CreateImageRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveProjects)) // TODO rights
      {
        return _responseCreater.CreateFailureResponse<List<Guid>>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      List<string> errors = validationResult.Errors.Select(vf => vf.ErrorMessage).ToList();

      if (!validationResult.IsValid)
      {
        return _responseCreater.CreateFailureResponse<List<Guid>>(HttpStatusCode.BadRequest, errors);
      }

      List<Guid> imagesIds = await CreateImagesAsync(
        request.Images, _httpContextAccessor.HttpContext.GetUserId(), errors);

      if (errors.Any())
      {
        return _responseCreater.CreateFailureResponse<List<Guid>>(HttpStatusCode.BadRequest, errors);
      }

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        Body = await _repository.CreateAsync(imagesIds.Select(imageId =>
          _dbProjectImageMapper.Map(request, imageId))
          .ToList())
      };
    }
  }
}
