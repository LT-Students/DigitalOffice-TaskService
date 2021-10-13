using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Business.Commands.Image.Interfaces
{
  [AutoInject]
  public interface IRemoveImageCommand
  {
    Task<OperationResultResponse<bool>> ExecuteAsync(RemoveImageRequest request);
  }
}
