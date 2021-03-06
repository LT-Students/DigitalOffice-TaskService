using LT.DigitalOffice.TaskService.Data.Provider;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LT.DigitalOffice.TaskService.Mappers.Helpers
{
  public static class TaskNumberHelper
  {
    private static ConcurrentDictionary<Guid, int> _cache = new();

    public static void LoadCache(IDataProvider provider)
    {
      var projectsIds = provider.Tasks.Select(x => x.ProjectId).Distinct().ToList();

      foreach (var projectId in projectsIds)
      {
        var tasks = provider.Tasks.Where(x => x.ProjectId == projectId).Select(x => x.Number).ToList();

        _cache[projectId] = tasks.Any() ? tasks.Max(x => x) : 0;
      }
    }

    public static int GetProjectTaskNumber(Guid projectId)
    {
      return _cache.ContainsKey(projectId) ? _cache[projectId] += 1 : _cache[projectId] = 1;
    }
  }
}
