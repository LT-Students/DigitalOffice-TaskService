using System;

namespace LT.DigitalOffice.TaskService.Models.Dto.Models
{
  public record UserInfo
  {
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; }
    public ImageInfo AvatarImage { get; set; }
  }
}
