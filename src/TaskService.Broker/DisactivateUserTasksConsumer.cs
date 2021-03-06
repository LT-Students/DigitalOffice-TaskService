using System.Threading.Tasks;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.Models.Broker.Publishing;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using MassTransit;

namespace LT.DigitalOffice.TaskService.Broker
{
  public class DisactivateUserTasksConsumer : IConsumer<IDisactivateUserPublish>
  {
    private readonly ITaskRepository _repository;

    public DisactivateUserTasksConsumer(
      ITaskRepository repository)
    {
      _repository = repository;
    }

    public async Task Consume(ConsumeContext<IDisactivateUserPublish> context)
    {
      await _repository.UserDisactivateAsync(context.Message.UserId, context.Message.ModifiedBy);
    }
  }
}
