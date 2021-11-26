using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models
{
  public class UserInfoMapper : IUserInfoMapper
  {
    public UserInfo Map(
      UserData userData,
      ImageInfo image)
    {
      if (userData == null)
      {
        return null;
      }

      return new UserInfo
      {
        Id = userData.Id,
        FirstName = userData.FirstName,
        LastName = userData.LastName,
        MiddleName = userData.MiddleName,
        IsActive = userData.IsActive,
        AvatarImage = image
      };
    }
  }
}
