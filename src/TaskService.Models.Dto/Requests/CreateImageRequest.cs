using System;
using System.Collections.Generic;
using LT.DigitalOffice.ProjectService.Models.Dto.Requests;

namespace LT.DigitalOffice.TaskService.Models.Dto.Requests
{
  public record CreateImageRequest
  {
    public Guid TaskId { get; set; }
    public List<ImageContent> Images { get; set; }
  }
}
