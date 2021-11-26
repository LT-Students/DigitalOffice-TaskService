using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models
{
  public class ImageInfoMapper : IImageInfoMapper
  {
    public ImageInfo Map(ImageData image)
    {
      if (image == null)
      {
        return null;
      }

      return new()
      {
        Id = image.ImageId,
        ParentId = image.ParentId,
        Type = image.Type,
        Name = image.Name,
        Content = image.Content,
        Extension = image.Extension
      };
    }
  }
}
