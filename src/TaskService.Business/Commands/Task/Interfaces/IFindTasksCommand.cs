using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces
{
  [AutoInject]
  public interface IFindTasksCommand
  {
    Task<FindResultResponse<TaskInfo>> ExecuteAsync(FindTasksFilter filter);
  }
}
