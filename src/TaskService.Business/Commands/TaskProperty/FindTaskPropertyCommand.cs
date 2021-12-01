using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;

namespace LT.DigitalOffice.ProjectService.Business.Commands
{
  public class FindTaskPropertyCommand : IFindTaskPropertyCommand
  {
    private readonly ITaskPropertyInfoMapper _mapper;
    private readonly ITaskPropertyRepository _repository;
    private readonly IBaseFindFilterValidator _findRequestValidator;
    private readonly IResponseCreator _responseCreator;

    public FindTaskPropertyCommand(
      ITaskPropertyRepository repository,
      ITaskPropertyInfoMapper mapper,
      IBaseFindFilterValidator findRequestValidator,
      IResponseCreator responseCreator)
    {
      _mapper = mapper;
      _repository = repository;
      _findRequestValidator = findRequestValidator;
      _responseCreator = responseCreator;
    }

    public async Task<FindResultResponse<TaskPropertyInfo>> ExecuteAsync(FindTaskPropertiesFilter filter)
    {
      if (!_findRequestValidator.ValidateCustom(filter, out List<string> errors))
      {
        return _responseCreator.CreateFailureFindResponse<TaskPropertyInfo>(HttpStatusCode.BadRequest, errors);
      }

      (List<DbTaskProperty> properties, int totalCount) = await _repository.FindAsync(filter);

      return new FindResultResponse<TaskPropertyInfo>
      {
        Body = properties.Select(_mapper.Map).ToList(),
        Status = OperationResultStatusType.FullSuccess,
        TotalCount = totalCount
      };
    }
  }
}
