using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Data.Provider;
using LT.DigitalOffice.TaskService.Models.Db;

namespace LT.DigitalOffice.TaskService.Data
{
  public class ImageRepository : IImageRepository
  {
    private readonly IDataProvider _provider;

    public ImageRepository(
      IDataProvider provider)
    {
      _provider = provider;
    }

    public async Task<List<Guid>> CreateAsync(IEnumerable<DbTaskImage> images)
    {
      if (images == null)
      {
        return null;
      }

      _provider.Images.AddRange(images);
      await _provider.SaveAsync();

      return images.Select(x => x.ImageId).ToList();
    }

    public async Task<bool> RemoveAsync(IEnumerable<Guid> imagesIds)
    {
      if (imagesIds == null)
      {
        return false;
      }

      _provider.Images.RemoveRange(_provider.Images
        .Where(x => imagesIds.Contains(x.ImageId)));
      await _provider.SaveAsync();

      return true;
    }
  }
}
