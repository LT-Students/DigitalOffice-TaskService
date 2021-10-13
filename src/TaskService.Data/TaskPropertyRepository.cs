using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Data.Provider;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Data
{
  public class TaskPropertyRepository : ITaskPropertyRepository
  {
    private readonly IDataProvider _provider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TaskPropertyRepository(
      IDataProvider provider,
      IHttpContextAccessor httpContextAccessor)
    {
      _provider = provider;
      _httpContextAccessor = httpContextAccessor;
    }

    public async Task CreateAsync(DbTaskProperty dbTaskProperty)
    {
      _provider.TaskProperties.Add(dbTaskProperty);
      await _provider.SaveAsync();
    }

    public bool DoExists(params Guid[] ids)
    {
      var dbIds = _provider.TaskProperties.Select(x => x.Id);

      return ids.All(x => dbIds.Contains(x));
    }

    public bool DoesExistForProject(Guid projectId, params string[] propertyNames)
    {
      var dbPropertyNames = _provider.TaskProperties
        .Where(tp => tp.ProjectId == projectId || tp.ProjectId == null)
        .Select(x => x.Name);

      return propertyNames.Any(x => dbPropertyNames.Contains(x));
    }

    public async Task<bool> EditAsync(DbTaskProperty taskProperty, JsonPatchDocument<DbTaskProperty> taskPatch)
    {
      taskPatch.ApplyTo(taskProperty);
      taskProperty.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      taskProperty.ModifiedAtUtc = DateTime.UtcNow;
      await _provider.SaveAsync();

      return true;
    }

    public DbTaskProperty Get(Guid propertyId)
    {
      return _provider.TaskProperties.FirstOrDefault(x => x.Id == propertyId);
    }

    public List<DbTaskProperty> Find(FindTaskPropertiesFilter filter, out int totalCount)
    {
      var dbTaskProperties = _provider.TaskProperties.AsQueryable();

      if (filter.ProjectId.HasValue)
      {
        dbTaskProperties = dbTaskProperties.Where(tp => tp.ProjectId == filter.ProjectId.Value || tp.ProjectId == null);
      }

      if (filter.AuthorId.HasValue)
      {
        dbTaskProperties = dbTaskProperties.Where(tp => tp.ProjectId == filter.AuthorId.Value);
      }

      if (!string.IsNullOrEmpty(filter.Name))
      {
        dbTaskProperties = dbTaskProperties.Where(tp => tp.Name.Contains(filter.Name));
      }

      if (filter.Type.HasValue)
      {
        dbTaskProperties = dbTaskProperties.Where(tp => tp.PropertyType == (int)filter.Type.Value);
      }

      totalCount = dbTaskProperties.Count();

      return dbTaskProperties.Skip(filter.SkipCount).Take(filter.TakeCount).ToList();
    }

    public bool DoesExist(Guid id, TaskPropertyType type)
    {
      return _provider.TaskProperties.Any(tp => tp.Id == id && tp.IsActive && tp.PropertyType == (int)type);
    }
  }
}
