using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.BrokerSupport.Broker;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.FluentValidationExtensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Extensions;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Kernel.Validators.Interfaces;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Requests.Filters;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task
{
  public class FindTasksCommand : IFindTasksCommand
  {
    private readonly ITaskInfoMapper _mapper;
    private readonly ITaskRepository _taskRepository;
    private readonly IBaseFindFilterValidator _findFilterValidator;
    private readonly ILogger<FindTasksCommand> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IResponseCreator _responseCreator;
    private readonly IRedisHelper _redisHelper;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<ICheckProjectUsersExistenceRequest> _rcCheckProjectUsers;

    #region private

    #region get projects

    private async Task<List<ProjectData>> GetProjectsAsync(Guid? projectId, Guid? userId, List<string> errors)
    {
      if (!projectId.HasValue && !userId.HasValue)
      {
        return null;
      }

      List<ProjectData> projects;
      if (projectId.HasValue)
      {
        (projects, _) = await _redisHelper.GetAsync<(List<ProjectData>, int)>(Cache.Projects, projectId.Value.GetRedisCacheHashCode());
      }
      else
      {
        (projects, _) = await _redisHelper.GetAsync<(List<ProjectData>, int)>(Cache.Projects, userId.Value.GetRedisCacheHashCode());
      }

      if (projects != null)
      {
        _logger.LogInformation("Projects info was taken from redis cache.");

        return projects;
      }

      return await GetProjectsThroughBrokerAsync(projectId, userId, errors);
    }

    private async Task<List<ProjectData>> GetProjectsThroughBrokerAsync(Guid? projectId, Guid? userId, List<string> errors)
    {
      if (!projectId.HasValue && !userId.HasValue)
      {
        return null;
      }

      string errorMessage = $"Can not get projects list for user '{userId}'. Please try again later.";

      try
      {
        object request;

        if (projectId.HasValue)
        {
          request = IGetProjectsRequest.CreateObj(projectsIds: new() { projectId.Value });
        }
        else
        {
          request = IGetProjectsRequest.CreateObj(userId: userId.Value);
        }

        Response<IOperationResult<IGetProjectsResponse>> response =
          await _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(request);

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("Projects info was taken (by userId {userId}) from service. ", userId);

          return response.Message.Body.Projects;
        }

        _logger.LogWarning(
          "Errors while getting projects list:\n{Errors}",
          string.Join('\n', response.Message.Errors));
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, "Can not get projects list for user '{UserId}'. Please try again later", userId);
      }

      errors.Add(errorMessage);

      return null;
    }

    #endregion

    private async Task<bool> DoesProjectUserExistAsync(Guid projectId, Guid userId)
    {
      string logMessage = "Cannot check project users existence.";

      try
      {
        Response<IOperationResult<List<Guid>>> response = await _rcCheckProjectUsers.GetResponse<IOperationResult<List<Guid>>>(
          ICheckProjectUsersExistenceRequest.CreateObj(projectId, new() { userId }));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body?.Any() ?? false;
        }

        _logger.LogWarning(logMessage);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      return false;
    }

    #endregion

    public FindTasksCommand(
      ITaskInfoMapper mapper,
      ITaskRepository taskRepository,
      IBaseFindFilterValidator findFilterValidator,
      ILogger<FindTasksCommand> logger,
      IHttpContextAccessor httpContextAccessor,
      IResponseCreator responseCreator,
      IRedisHelper redisHelper,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<ICheckProjectUsersExistenceRequest> rcCheckProjectUsers)
    {
      _mapper = mapper;
      _taskRepository = taskRepository;
      _findFilterValidator = findFilterValidator;
      _logger = logger;
      _httpContextAccessor = httpContextAccessor;
      _responseCreator = responseCreator;
      _redisHelper = redisHelper;
      _rcGetProjects = rcGetProjects;
      _rcCheckProjectUsers = rcCheckProjectUsers;
    }

    public async Task<FindResultResponse<TaskInfo>> ExecuteAsync(FindTasksFilter filter)
    {
      Guid userId = _httpContextAccessor.HttpContext.GetUserId();
      if (filter.ProjectId.HasValue
        && !await DoesProjectUserExistAsync(filter.ProjectId.Value, _httpContextAccessor.HttpContext.GetUserId()))
      {
        return _responseCreator.CreateFailureFindResponse<TaskInfo>(HttpStatusCode.Forbidden);
      }

      if (!_findFilterValidator.ValidateCustom(filter, out var errors))
      {
        _responseCreator.CreateFailureFindResponse<TaskInfo>(HttpStatusCode.BadRequest, errors);
      }

      List<ProjectData> projectDatas = await GetProjectsAsync(filter.ProjectId, userId, errors);

      FindResultResponse<TaskInfo> response = new();

      (List<DbTask> dbTasks, int totalCount) = await _taskRepository.FindAsync(filter, projectDatas.Select(p => p.Id).ToList());

      response.TotalCount = totalCount;
      response.Body = dbTasks.Select(t => _mapper.Map(t, projectDatas?.FirstOrDefault(p => p.Id == t.ProjectId))).ToList();

      return response;
    }
  }
}
