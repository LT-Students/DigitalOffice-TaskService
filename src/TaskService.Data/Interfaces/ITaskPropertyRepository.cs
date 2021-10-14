using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using Microsoft.AspNetCore.JsonPatch;

namespace LT.DigitalOffice.TaskService.Data.Interfaces
{
  [AutoInject]
  public interface ITaskPropertyRepository
  {
    Task CreateAsync(DbTaskProperty dbTaskProperty);

    Task<bool> DoesExistAsync(Guid id, TaskPropertyType type);

    Task<bool> DoesExistNameAsync(Guid projectId, string propertyName);

    Task<DbTaskProperty> GetAsync(Guid propertyId);

    Task<(List<DbTaskProperty>, int totalCount)> FindAsync(FindTaskPropertiesFilter filter);

    Task<bool> EditAsync(DbTaskProperty taskProperty, JsonPatchDocument<DbTaskProperty> taskPatch);
  }
}
