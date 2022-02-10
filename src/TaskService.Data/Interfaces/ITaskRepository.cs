using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.ProjectService.Data.Interfaces
{
  [AutoInject]
  public interface ITaskRepository
  {
    Task<DbTask> GetAsync(Guid taskId, bool isFullModel);

    Task<bool> EditAsync(DbTask task, JsonPatchDocument<DbTask> taskPatch);

    Task<(List<DbTask>, int totalCount)> FindAsync(FindTasksFilter filter, List<Guid> projectsIds);

    Task<Guid> CreateAsync(DbTask dbTask);

    Task<bool> DoesExistAsync(Guid id);

    Task UserDisactivateAsync(Guid userId, Guid modifiedBy);
  }
}
