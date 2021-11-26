using System;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;

namespace LT.DigitalOffice.TaskService.Models.Dto.Requests
{
  public record CreateTaskPropertyRequest
  {
    public Guid ProjectId { get; set; }
    public TaskPropertyType PropertyType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
  }
}
