using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.TaskService.Business.Commands.Image.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Image.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Image
{
  public class RemoveImageCommand : IRemoveImageCommand
  {
    private readonly IImageRepository _repository;
    private readonly IRequestClient<IRemoveImagesRequest> _rcImages;
    private readonly ILogger<RemoveImageCommand> _logger;
    private readonly IAccessValidator _accessValidator;
    private readonly IRemoveImageRequestValidator _validator;
    private readonly IResponseCreater _responseCreater;

    private bool RemoveImage(List<Guid> ids, List<string> errors)
    {
      if (ids == null || !ids.Any())
      {
        return false;
      }

      var logMessage = "Errors while removing images ids {ids}. Errors: {Errors}";

      try
      {
        var response = _rcImages.GetResponse<IOperationResult<bool>>(
          IRemoveImagesRequest.CreateObj(ids, ImageSource.Project)).Result.Message;

        if (response.IsSuccess)
        {
          return true;
        }

        _logger.LogWarning(
          logMessage,
          string.Join('\n', ids),
          string.Join('\n', response.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      errors.Add("Can not remove images. Please try again later.");

      return false;
    }

    public RemoveImageCommand(
      IImageRepository repository,
      IRequestClient<IRemoveImagesRequest> rcImages,
      ILogger<RemoveImageCommand> logger,
      IAccessValidator accessValidator,
      IRemoveImageRequestValidator validator,
      IResponseCreater responseCreater)
    {
      _repository = repository;
      _rcImages = rcImages;
      _logger = logger;
      _accessValidator = accessValidator;
      _validator = validator;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<bool>> ExecuteAsync(RemoveImageRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveProjects)) // TODO rights
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.Forbidden);
      }

      if (!_validator.ValidateCustom(request, out var errors))
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      var result = RemoveImage(request.ImagesIds, errors);

      if (!result)
      {
        return _responseCreater.CreateFailureResponse<bool>(HttpStatusCode.BadRequest, errors);
      }

      return new()
      {
        Status = OperationResultStatusType.FullSuccess,
        Body = await _repository.RemoveAsync(request.ImagesIds)
      };
    }
  }
}
