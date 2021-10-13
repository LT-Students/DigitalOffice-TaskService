using System.Collections.Generic;
using System.Linq;
using System.Net;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;

namespace LT.DigitalOffice.ProjectService.Business.Commands
{
  public class FindTaskPropertyCommand : IFindTaskPropertyCommand
  {
    private readonly ITaskPropertyInfoMapper _mapper;
    private readonly ITaskPropertyRepository _repository;
    private readonly IBaseFindFilterValidator _findRequestValidator;
    private readonly IResponseCreater _responseCreater;

    public FindTaskPropertyCommand(
      ITaskPropertyRepository repository,
      ITaskPropertyInfoMapper mapper,
      IBaseFindFilterValidator findRequestValidator,
      IResponseCreater responseCreater)
    {
      _mapper = mapper;
      _repository = repository;
      _findRequestValidator = findRequestValidator;
      _responseCreater = responseCreater;
    }

    public FindResultResponse<TaskPropertyInfo> Execute(FindTaskPropertiesFilter filter)
    {
      if (!_findRequestValidator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreater.CreateFailureFindResponse<TaskPropertyInfo>(HttpStatusCode.BadRequest, errors);
      }

      return new FindResultResponse<TaskPropertyInfo>
      {
        Body = _repository
          .Find(filter, out int totalCount)
          .Select(tp => _mapper.Map(tp))
          .ToList(),
        Status = OperationResultStatusType.FullSuccess,
        TotalCount = totalCount
      };
    }
  }
}
