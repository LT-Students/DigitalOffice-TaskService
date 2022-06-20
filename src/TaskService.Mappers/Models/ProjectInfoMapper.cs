using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models
{
  public class ProjectInfoMapper : IProjectInfoMapper
  {
    public ProjectInfo Map(ProjectData projectData)
    {
      if (projectData == null)
      {
        return null;
      }

      return new ProjectInfo
      {
        Id = projectData.Id,
        Name = projectData.Name,
        Status = projectData.Status,
        ShortName = projectData.ShortName
      };
    }
  }
}

