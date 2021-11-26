using System.Collections.Generic;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Models.Dto.Responses
{
  public record TaskResponse
  {
    public TaskInfo Task { get; set; }
    public UserInfo AssignedTo { get; set; }
    public UserInfo CreatedBy { get; set; }
    public string Description { get; set; }
    public TaskPropertyInfo Status { get; set; }
    public TaskPropertyInfo Priority { get; set; }
    public TaskPropertyInfo Type { get; set; }
    public TaskInfo ParentTask { get; set; }
    public IEnumerable<TaskInfo> Subtasks { get; set; }
    public IEnumerable<ImageInfo> TaskImages { get; set; }
  }
}
