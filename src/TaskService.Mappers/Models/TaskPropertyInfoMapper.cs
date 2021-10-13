using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models
{
  public class TaskPropertyInfoMapper : ITaskPropertyInfoMapper
  {
    public TaskPropertyInfo Map(DbTaskProperty dbTaskProperty)
    {
      if (dbTaskProperty == null)
      {
        return null;
      }

      return new TaskPropertyInfo
      {
        Id = dbTaskProperty.Id,
        ProjectId = dbTaskProperty.ProjectId,
        CreatedBy = dbTaskProperty.CreatedBy,
        Name = dbTaskProperty.Name,
        CreatedAtUtc = dbTaskProperty.CreatedAtUtc,
        Description = dbTaskProperty.Description,
        IsActive = dbTaskProperty.IsActive,
        PropertyType = (TaskPropertyType)dbTaskProperty.PropertyType
      };
    }
  }
}
