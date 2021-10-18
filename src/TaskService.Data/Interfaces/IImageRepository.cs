using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.TaskService.Models.Db;

namespace LT.DigitalOffice.TaskService.Data.Interfaces
{
  [AutoInject]
  public interface IImageRepository
  {
    Task<List<Guid>> CreateAsync(IEnumerable<DbTaskImage> images);

    Task<bool> RemoveAsync(IEnumerable<Guid> imagesIds);
  }
}
