using System;
using System.Collections.Generic;

namespace LT.DigitalOffice.TaskService.Models.Dto.Requests
{
  public class RemoveImageRequest
  {
    public Guid TaskId { get; set; }
    public List<Guid> ImagesIds { get; set; }
  }
}
