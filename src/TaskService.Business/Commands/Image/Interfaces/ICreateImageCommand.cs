using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LT.DigitalOffice.TaskService.Business.Commands.Image.Interfaces
{
  [AutoInject]
  public interface ICreateImageCommand
  {
    Task<OperationResultResponse<List<Guid>>> ExecuteAsync(CreateImageRequest request);
  }
}
