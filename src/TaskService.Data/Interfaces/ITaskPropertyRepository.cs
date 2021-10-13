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

    bool DoesExist(Guid id, TaskPropertyType type);

    bool DoesExistForProject(Guid projectId, params string[] propertyNames);

    DbTaskProperty Get(Guid propertyId);

    List<DbTaskProperty> Find(FindTaskPropertiesFilter filter, out int totalCount);

    Task<bool> EditAsync(DbTaskProperty taskProperty, JsonPatchDocument<DbTaskProperty> taskPatch);
  }
}
