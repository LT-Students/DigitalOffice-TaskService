using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Models.Dto.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces;

namespace LT.DigitalOffice.TaskService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class TaskController : ControllerBase
  {
    [HttpGet("find")]
    public async Task<FindResultResponse<TaskInfo>> FindAsync(
      [FromServices] IFindTasksCommand command,
      [FromQuery] FindTasksFilter filter)
    {
      return await command.ExecuteAsync(filter);
    }

    [HttpGet("get")]
    public async Task<OperationResultResponse<TaskResponse>> GetAsync(
      [FromQuery] Guid taskId,
      [FromServices] IGetTaskCommand command)
    {
      return await command.ExecuteAsync(taskId);
    }

    [HttpPost("create")]
    public async Task<OperationResultResponse<Guid>> CreateAsync(
      [FromServices] ICreateTaskCommand command,
      [FromBody] CreateTaskRequest request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpPatch("edit")]
    public async Task<OperationResultResponse<bool>> EditAsync(
      [FromQuery] Guid taskId,
      [FromBody] JsonPatchDocument<EditTaskRequest> request,
      [FromServices] IEditTaskCommand command)
    {
      return await command.ExecuteAsync(taskId, request);
    }
  }
}
