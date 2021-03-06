using LT.DigitalOffice.Kernel.BrokerSupport.Attributes;
using LT.DigitalOffice.Kernel.BrokerSupport.Configurations;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.Models.Broker.Publishing.Subscriber.Image;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;

namespace LT.DigitalOffice.ProjectService.Models.Dto.Configurations
{
  public class RabbitMqConfig : BaseRabbitMqConfig
  {
    // Subscriptions
    public string DisactivateUserTasksEndpoint { get; set; }

    // common

    [AutoInjectRequest(typeof(ICheckProjectsExistence))]
    public string CheckProjectsExistenceEndpoint { get; set; }

    // project

    [AutoInjectRequest(typeof(IGetProjectsRequest))]
    public string GetProjectsEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetProjectsUsersRequest))]
    public string GetProjectsUsersEndpoint { get; set; }

    [AutoInjectRequest(typeof(ICheckProjectUsersExistenceRequest))]
    public string CheckProjectUsersExistenceEndpoint { get; set; }

    // image

    [AutoInjectRequest(typeof(IRemoveImagesPublish))]
    public string RemoveImagesEndpoint { get; set; }

    [AutoInjectRequest(typeof(IGetImagesRequest))]
    public string GetImagesEndpoint { get; set; }

    [AutoInjectRequest(typeof(ICreateImagesRequest))]
    public string CreateImagesEndpoint { get; set; }

    // user

    [AutoInjectRequest(typeof(IGetUsersDataRequest))]
    public string GetUsersDataEndpoint { get; set; }
  }
}
