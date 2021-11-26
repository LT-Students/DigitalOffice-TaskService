using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Business.Commands.Image.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LT.DigitalOffice.TaskService.Controllers
{
  [Route("[controller]")]
  [ApiController]
  public class ImageController : ControllerBase
  {
    [HttpPost("create")]
    public async Task<OperationResultResponse<List<Guid>>> CreateAsync(
      [FromServices] ICreateImageCommand command,
      [FromBody] CreateImageRequest request)
    {
      return await command.ExecuteAsync(request);
    }

    [HttpDelete("remove")]
    public async Task<OperationResultResponse<bool>> RemoveAsync(
      [FromServices] IRemoveImageCommand command,
      [FromBody] RemoveImageRequest request)
    {
      return await command.ExecuteAsync(request);
    }
  }
}
