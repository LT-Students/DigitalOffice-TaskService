using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Data.Provider;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;

namespace LT.DigitalOffice.ProjectService.Data
{
  public class TaskRepository : ITaskRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private IQueryable<DbTask> CreateFindPredicates(
      FindTasksFilter filter,
      List<Guid> projectsIds,
      IQueryable<DbTask> dbTasks)
    {
      if (filter.Number.HasValue)
      {
        dbTasks = dbTasks.Where(x => x.Number.Equals(filter.Number));
      }

      if (filter.ProjectId.HasValue)
      {
        dbTasks = dbTasks.Where(x => x.ProjectId.Equals(filter.ProjectId));
      }

      if (filter.AssignedTo.HasValue)
      {
        dbTasks = dbTasks.Where(x => x.AssignedTo.Equals(filter.AssignedTo));
      }

      if (filter.Status.HasValue)
      {
        dbTasks = dbTasks.Where(x => x.Status.Id.Equals(filter.Status));
      }

      if (projectsIds != null)
      {
        dbTasks = dbTasks.Where(x => projectsIds.Contains(x.ProjectId));
      }

      return dbTasks;
    }

    public TaskRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> CreateAsync(DbTask newTask)
    {
      _provider.Tasks.Add(newTask);
      await _provider.SaveAsync();

      return newTask.Id;
    }

    public async Task<bool> DoesExistAsync(Guid id)
    {
      return await _provider.Tasks.FirstOrDefaultAsync(x => x.Id == id) != null;
    }

    public async Task<bool> EditAsync(DbTask task, JsonPatchDocument<DbTask> taskPatch)
    {
      taskPatch.ApplyTo(task);
      task.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      task.ModifiedAtUtc = DateTime.UtcNow;
      await _provider.SaveAsync();

      return true;
    }

    public async Task<DbTask> GetAsync(Guid taskId, bool isFullModel)
    {
      if (isFullModel)
      {
        return await _provider.Tasks
          .Include(t => t.Status)
          .Include(t => t.Priority)
          .Include(t => t.Type)
          .Include(t => t.Subtasks)
          .Include(t => t.Images)
          .Include(t => t.Parent)
          .FirstOrDefaultAsync(x => x.Id == taskId);
      }

      return await _provider.Tasks.FirstOrDefaultAsync(x => x.Id == taskId);
    }

    public async Task<(List<DbTask>, int totalCount)> FindAsync(
      FindTasksFilter filter,
      List<Guid> projectsIds)
    {
      if (filter == null)
      {
        return (null, 0);
      }

      IQueryable<DbTask> dbTasks = _provider.Tasks
        .Include(t => t.Priority)
        .Include(t => t.Type)
        .Include(t => t.Status)
        .AsSingleQuery()
        .AsQueryable();

      IQueryable<DbTask> tasks = CreateFindPredicates(filter, projectsIds, dbTasks);
      int totalCount = await tasks.CountAsync();

      return (await tasks.Skip(filter.SkipCount).Take(filter.TakeCount).ToListAsync(), totalCount);
    }

    public async Task<bool> UserDisactivateAsync(Guid userId, Guid modifiedBy)
    {
      IQueryable<DbTask> dbTasks = _provider.Tasks.Where(u => u.AssignedTo == userId).AsQueryable();

      if (!dbTasks.Any())
      {
        return false;
      }

      foreach (DbTask task in dbTasks)
      {
        task.AssignedTo = null;
        task.ModifiedBy = modifiedBy;
        task.ModifiedAtUtc = DateTime.UtcNow;
      } 
      await _provider.SaveAsync();

      return true;
    }
  }
}
