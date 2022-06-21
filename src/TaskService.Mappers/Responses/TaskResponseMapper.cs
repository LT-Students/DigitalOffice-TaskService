using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Project;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Responses;

namespace LT.DigitalOffice.TaskService.Mappers.Responses
{
  public class TaskResponseMapper : ITaskResponseMapper
  {
    private readonly ITaskInfoMapper _taskInfoMapper;
    private readonly ITaskPropertyInfoMapper _taskPropertyInfoMapper;
    private readonly IUserInfoMapper _userMapper;

    public TaskResponseMapper(
      ITaskInfoMapper taskInfoMapper,
      ITaskPropertyInfoMapper taskPropertyInfoMapper,
      IUserInfoMapper userMapper)
    {
      _taskInfoMapper = taskInfoMapper;
      _taskPropertyInfoMapper = taskPropertyInfoMapper;
      _userMapper = userMapper;
    }

    public TaskResponse Map(
      DbTask dbTask,
      ProjectData project,
      List<UserData> users,
      List<ImageInfo> taskImages,
      List<ImageInfo> usersImages)
    {
      if (dbTask == null)
      {
        return null;
      }

      UserData assignedTo = users?.FirstOrDefault(u => u.Id == dbTask.AssignedTo);
      UserData createdBy = users?.FirstOrDefault(u => u.Id == dbTask.CreatedBy);

      return new TaskResponse()
      {
        Task = _taskInfoMapper.Map(dbTask, project),
        AssignedTo = _userMapper.Map(assignedTo, usersImages?.FirstOrDefault(i => i.Id == assignedTo.ImageId)),
        CreatedBy = _userMapper.Map(createdBy, usersImages?.FirstOrDefault(i => i.Id == createdBy.ImageId)),
        Description = dbTask.Description,
        Status = _taskPropertyInfoMapper.Map(dbTask?.Status),
        Priority = _taskPropertyInfoMapper.Map(dbTask?.Priority),
        Type = _taskPropertyInfoMapper.Map(dbTask?.Type),
        ParentTask = _taskInfoMapper.Map(dbTask?.Parent, project),
        Subtasks = dbTask.Subtasks?.Select(t => _taskInfoMapper.Map(t, project)).ToList(),
        TaskImages = taskImages
      };
    }
  }
}
