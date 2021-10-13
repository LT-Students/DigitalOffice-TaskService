using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.ProjectService.Data.Interfaces
{
  /// <summary>
  /// Represents interface of repository in repository pattern.
  /// Provides methods for working with the database of TaskService.
  /// </summary>
  [AutoInject]
  public interface ITaskRepository
  {
    DbTask Get(Guid taskId, bool isFullModel);

    Task<bool> EditAsync(DbTask task, JsonPatchDocument<DbTask> taskPatch);

    List<DbTask> Find(FindTasksFilter filter, List<Guid> projectsIds, out int totalCount);

    Task<Guid> CreateAsync(DbTask dbTask);

    bool DoesExist(Guid id);
  }
}
