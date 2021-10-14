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
using Microsoft.EntityFrameworkCore;

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

    public async Task<bool> DoesExistNameAsync(Guid projectId, string propertyName)
    {
      return await _provider.TaskProperties.AnyAsync(tp => tp.IsActive && tp.Name == propertyName && tp.ProjectId == projectId);
    }

    public async Task<bool> EditAsync(DbTaskProperty taskProperty, JsonPatchDocument<DbTaskProperty> taskPatch)
    {
      taskPatch.ApplyTo(taskProperty);
      taskProperty.ModifiedBy = _httpContextAccessor.HttpContext.GetUserId();
      taskProperty.ModifiedAtUtc = DateTime.UtcNow;
      await _provider.SaveAsync();

      return true;
    }

    public async Task<DbTaskProperty> GetAsync(Guid propertyId)
    {
      return await _provider.TaskProperties.FirstOrDefaultAsync(x => x.Id == propertyId);
    }

    public async Task<(List<DbTaskProperty>, int totalCount)> FindAsync(FindTaskPropertiesFilter filter)
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

      int totalCount = await dbTaskProperties.CountAsync();

      return (await dbTaskProperties.Skip(filter.SkipCount).Take(filter.TakeCount).ToListAsync(), totalCount);
    }

    public async Task<bool> DoesExistAsync(Guid id, TaskPropertyType type)
    {
      return await _provider.TaskProperties.AnyAsync(tp => tp.Id == id && tp.IsActive && tp.PropertyType == (int)type);
    }
  }
}
