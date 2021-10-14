using System;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Business.Commands.TaskProperty.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TaskService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class TaskPropertyController : ControllerBase
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<Guid>> Create(
      [FromServices] ICreateTaskPropertyCommand command,
      [FromBody] CreateTaskPropertyRequest request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpGet("find")]
    public async Task<FindResultResponse<TaskPropertyInfo>> Find(
      [FromServices] IFindTaskPropertyCommand command,
      [FromQuery] FindTaskPropertiesFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> Edit(
      [FromQuery] Guid taskPropertyId,
      [FromBody] JsonPatchDocument<EditTaskPropertyRequest> request,
      [FromServices] IEditTaskPropertyCommand command)
    {
      return await command.ExecuteAsync(taskPropertyId, request);
    }
  }
}
