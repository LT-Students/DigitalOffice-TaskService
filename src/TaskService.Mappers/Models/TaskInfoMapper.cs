using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models
{
  public class TaskInfoMapper : ITaskInfoMapper
  {
    private readonly IProjectInfoMapper _projectInfoMapper;

    public TaskInfoMapper(IProjectInfoMapper projectInfoMapper)
    {
      _projectInfoMapper = projectInfoMapper;
    }

    public TaskInfo Map(DbTask dbTask, ProjectData project)
    {
      if (dbTask == null)
      {
        return null;
      }

      return new TaskInfo
      {
        Id = dbTask.Id,
        Name = dbTask.Name,
        Number = dbTask.Number,
        TypeName = dbTask.Type?.Name,
        CreatedAtUtc = dbTask.CreatedAtUtc,
        StatusName = dbTask.Status?.Name,
        PriorityName = dbTask.Priority?.Name,
        PlannedMinutes = dbTask.PlannedMinutes,
        Project = _projectInfoMapper.Map(project)
      };
    }
  }
}
