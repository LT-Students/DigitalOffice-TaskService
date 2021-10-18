using System;

namespace LT.DigitalOffice.TaskService.Models.Dto.Models
{
  public record TaskInfo
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string TypeName { get; set; }
    public string StatusName { get; set; }
    public string PriorityName { get; set; }
    public int Number { get; set; }
    public int? PlannedMinutes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public ProjectInfo Project { get; set; }
  }
}
