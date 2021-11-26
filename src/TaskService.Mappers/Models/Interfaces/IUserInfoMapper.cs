using LT.DigitalOffice.Kernel.Attributes;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.TaskService.Models.Dto.Models;

namespace LT.DigitalOffice.TaskService.Mappers.Models.Interfaces
{
  [AutoInject]
  public interface IUserInfoMapper
  {
    UserInfo Map(
      UserData userData,
      ImageInfo image);
  }
}
