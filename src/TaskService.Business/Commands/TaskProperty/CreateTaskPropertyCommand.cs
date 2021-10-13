using System;
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
using LT.DigitalOffice.TaskService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.TaskProperty.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.ProjectService.Business.Commands
{
  public class CreateTaskPropertyCommand : ICreateTaskPropertyCommand
  {
    private readonly IDbTaskPropertyMapper _mapper;
    private readonly IAccessValidator _accessValidator;
    private readonly ICreateTaskPropertyRequestValidator _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITaskPropertyRepository _taskPropertyRepository;
    private readonly IResponseCreater _responseCreater;

    public CreateTaskPropertyCommand(
      IDbTaskPropertyMapper mapper,
      IAccessValidator accessValidator,
      ITaskPropertyRepository taskPropertyRepository,
      ICreateTaskPropertyRequestValidator validator,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreater responseCreater)
    {
      _mapper = mapper;
      _validator = validator;
      _accessValidator = accessValidator;
      _taskPropertyRepository = taskPropertyRepository;
      _httpContextAccessor = httpContextAccessor;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<Guid>> ExecuteAsync(CreateTaskPropertyRequest request)
    {
      if (!await _accessValidator.HasRightsAsync(Rights.AddEditRemoveProjects))
      {
        return _responseCreater.CreateFailureResponse<Guid>(HttpStatusCode.Forbidden);
      }

      ValidationResult validationResult = await _validator.ValidateAsync(request);

      if (!validationResult.IsValid)
      {
        return _responseCreater.CreateFailureResponse<Guid>(
          HttpStatusCode.BadRequest, validationResult.Errors.Select(vf => vf.ErrorMessage).ToList());
      }

      DbTaskProperty dbTaskProperty = _mapper.Map(request);

      await _taskPropertyRepository.CreateAsync(dbTaskProperty);

      _httpContextAccessor.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

      return new OperationResultResponse<Guid>
      {
        Body = dbTaskProperty.Id,
        Status = OperationResultStatusType.FullSuccess
      };
    }
  }
}
