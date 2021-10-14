using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LT.DigitalOffice.Kernel.AccessValidatorEngine.Interfaces;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Constants;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Helpers.Interfaces;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.Models.Broker.Enums;
using LT.DigitalOffice.Models.Broker.Models;
using LT.DigitalOffice.Models.Broker.Models.Company;
using LT.DigitalOffice.Models.Broker.Requests.Company;
using LT.DigitalOffice.Models.Broker.Requests.Image;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.Models.Broker.Requests.User;
using LT.DigitalOffice.Models.Broker.Responses.Company;
using LT.DigitalOffice.Models.Broker.Responses.Image;
using LT.DigitalOffice.Models.Broker.Responses.Project;
using LT.DigitalOffice.Models.Broker.Responses.User;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Business.Commands.Task.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Models.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Responses.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Models;
using LT.DigitalOffice.TaskService.Models.Dto.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Business.Commands.Task
{
  public class GetTaskCommand : IGetTaskCommand
  {
    private readonly ITaskRepository _taskRepository;
    private readonly IAccessValidator _accessValidator;
    private readonly HttpContext _httpContext;
    private readonly ITaskResponseMapper _taskResponseMapper;
    private readonly ITaskInfoMapper _taskInfoMapper;
    private readonly IImageInfoMapper _imageMapper;
    private readonly ILogger<GetTaskCommand> _logger;
    private readonly IRequestClient<IGetImagesRequest> _rcImages;
    private readonly IRequestClient<IGetUsersDataRequest> _rcGetUsers;
    private readonly IRequestClient<IGetProjectsRequest> _rcGetProjects;
    private readonly IRequestClient<IGetDepartmentsRequest> _rcGetDepartment;
    private readonly IRequestClient<ICheckProjectUsersExistenceRequest> _rcCheckProjectUsers;
    private readonly IRedisHelper _redisHelper;
    private readonly IResponseCreater _responseCreater;

    #region private

    #region get departments

    private async Task<DepartmentData> GetDepartmentAsync(Guid departmentId, List<string> errors)
    {
      DepartmentData department =
        (await _redisHelper.GetAsync<List<DepartmentData>>(Cache.Departments, departmentId.GetRedisCacheHashCode()))?.FirstOrDefault();

      if (department != null)
      {
        _logger.LogInformation("Department (with id {departmentId}) was taken from redis.", departmentId);

        return department;
      }

      return await GetDepartmentThroughBrokerAsync(departmentId, errors);
    }

    private async Task<DepartmentData> GetDepartmentThroughBrokerAsync(Guid departmentId, List<string> errors)
    {
      var errorMessage = "Cannot check rights. Please try again later.";

      try
      {
        Response<IOperationResult<IGetDepartmentsResponse>> response =
          await _rcGetDepartment.GetResponse<IOperationResult<IGetDepartmentsResponse>>(
           IGetDepartmentsRequest.CreateObj(new() { departmentId }));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("Department (with id {departmentId}) was taken through broker.", departmentId);

          return response.Message.Body.Departments.FirstOrDefault();
        }

        _logger.LogWarning("Can not find department with Id: '{departmentId}'", departmentId);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, errorMessage);
      }

      errors.Add(errorMessage);

      return null;
    }

    #endregion

    #region get usersDatas

    private async Task<List<UserData>> GetUsersDatasAsync(List<Guid> usersIds, List<string> errors)
    {
      if (usersIds == null && !usersIds.Any())
      {
        return null;
      }

      List<UserData> usersFromCache = await _redisHelper.GetAsync<List<UserData>>(Cache.Users, usersIds.GetRedisCacheHashCode());

      if (usersFromCache != null)
      {
        _logger.LogInformation("UsersDatas were taken from the redis cache. Users ids: {usersIds}", string.Join(", ", usersIds));

        return usersFromCache;
      }

      return await GetUsersDatasThroughBrokerAsync(usersIds, errors);
    }

    private async Task<List<UserData>> GetUsersDatasThroughBrokerAsync(List<Guid> usersIds, List<string> errors)
    {
      var errorMessage = "Can not find user data. Please try again later.";

      try
      {
        var response = await _rcGetUsers.GetResponse<IOperationResult<IGetUsersDataResponse>>(
          IGetUsersDataRequest.CreateObj(usersIds));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("UsersDatas were taken from the service. Users ids: {usersIds}", string.Join(", ", usersIds));

          return response.Message.Body.UsersData;
        }

        errors.AddRange(response.Message.Errors);

        _logger.LogWarning("Can not find user data with this id {UserId}: " +
          $"{Environment.NewLine}{string.Join('\n', response.Message.Errors)}", usersIds);
      }
      catch (Exception exc)
      {
        errors.Add(errorMessage);

        _logger.LogError(exc, errorMessage);
      }

      return null;
    }

    #endregion

    #region get projects

    private async Task<ProjectData> GetProjectAsync(Guid userId, List<string> errors)
    {
      (List<ProjectData> projects, int _) = await _redisHelper.GetAsync<(List<ProjectData>, int)>(Cache.Projects, userId.GetRedisCacheHashCode());

      if (projects != null)
      {
        _logger.LogInformation("Projects info was taken (by userId {userId}) from redis cache. ", userId);

        return projects.FirstOrDefault();
      }

      return await GetProjectThroughBrokerAsync(userId, errors);
    }

    private async Task<ProjectData> GetProjectThroughBrokerAsync(Guid userId, List<string> errors)
    {
      string errorMessage = $"Can not get projects list for user '{userId}'. Please try again later.";

      try
      {
        Response<IOperationResult<IGetProjectsResponse>> response = await _rcGetProjects.GetResponse<IOperationResult<IGetProjectsResponse>>(
          IGetProjectsRequest.CreateObj(userId: userId));

        if (response.Message.IsSuccess)
        {
          _logger.LogInformation("Projects info was taken (by userId {userId}) from service. ", userId);

          return response.Message.Body.Projects?.FirstOrDefault();
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

    #region images

    private async Task<List<ImageInfo>> GetImagesAsync(List<Guid> imageIds, ImageSource source, List<string> errors)
    {
      if (imageIds == null || !imageIds.Any())
      {
        return null;
      }

      var logMessage = "Errors while getting images with ids: {Ids}. Errors: {Errors}";

      try
      {
        var response =
          await _rcImages.GetResponse<IOperationResult<IGetImagesResponse>>(
            IGetImagesRequest.CreateObj(imageIds, source));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ImagesData?.Select(_imageMapper.Map).ToList();
        }
        else
        {
          _logger.LogWarning(
            logMessage,
            string.Join(", ", imageIds),
            string.Join('\n', response.Message.Errors));
        }
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage, string.Join(", ", imageIds));
      }

      errors.Add("Can not get images. Please try again later.");

      return null;
    }

    #endregion

    private async Task<bool> DoesProjectUserExist(Guid projectId, Guid userId)
    {
      var logMessage = "Cannot check project users existence.";

      try
      {
        var response = await _rcCheckProjectUsers.GetResponse<IOperationResult<List<Guid>>>(
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

    private async Task<(bool hasRights, ProjectData project)> Authorization(Guid projectId, List<string> errors)
    {
      var requestUserId = _httpContext.GetUserId();

      ProjectData project = await GetProjectAsync(projectId, errors);

      if (await DoesProjectUserExist(projectId, requestUserId)
        || await _accessValidator.IsAdminAsync()
        || (await GetDepartmentAsync(requestUserId, errors))?.DirectorUserId == requestUserId) // do we need this?
      {
        return (true, project);
      }

      return (false, null);
    }

    #endregion

    public GetTaskCommand(
      ITaskRepository taskRepository,
      IAccessValidator accessValidator,
      IHttpContextAccessor httpContextAccessor,
      ITaskResponseMapper taskResponseMapper,
      IImageInfoMapper imageMapper,
      ITaskInfoMapper taskInfoMapper,
      IRequestClient<ICheckProjectUsersExistenceRequest> rcCheckProjectUsers,
      IRequestClient<IGetDepartmentsRequest> rcGetDepartment,
      IRequestClient<IGetProjectsRequest> rcGetProjects,
      IRequestClient<IGetUsersDataRequest> rcGetUsers,
      IRequestClient<IGetImagesRequest> rcImages,
      ILogger<GetTaskCommand> logger,
      IRedisHelper redisHelper,
      IResponseCreater responseCreater)
    {
      _taskRepository = taskRepository;
      _accessValidator = accessValidator;
      _httpContext = httpContextAccessor.HttpContext;
      _taskResponseMapper = taskResponseMapper;
      _taskInfoMapper = taskInfoMapper;
      _imageMapper = imageMapper;
      _rcCheckProjectUsers = rcCheckProjectUsers;
      _rcGetDepartment = rcGetDepartment;
      _rcGetProjects = rcGetProjects;
      _rcGetUsers = rcGetUsers;
      _rcImages = rcImages;
      _logger = logger;
      _redisHelper = redisHelper;
      _responseCreater = responseCreater;
    }

    public async Task<OperationResultResponse<TaskResponse>> ExecuteAsync(Guid taskId)
    {
      List<string> errors = new();

      DbTask dbTask = await _taskRepository.GetAsync(taskId, isFullModel: true);

      if (dbTask == null)
      {
        return _responseCreater.CreateFailureResponse<TaskResponse>(HttpStatusCode.NotFound);
      }

      (bool hasRights, ProjectData project) = await Authorization(dbTask.ProjectId, errors);

      if (!hasRights)
      {
        errors.Add("Not enough rights.");

        return _responseCreater.CreateFailureResponse<TaskResponse>(HttpStatusCode.Forbidden, errors);
      }

      List<Guid> userIds = new() { dbTask.CreatedBy };

      if (dbTask.AssignedTo.HasValue)
      {
        userIds.Add(dbTask.AssignedTo.Value);
      }

      List<UserData> usersDatas = await GetUsersDatasAsync(userIds, errors);

      List<Guid> taskImagesIds = dbTask.Images.Select(x => x.ImageId).ToList();

      List<ImageInfo> taskImagesInfo = await GetImagesAsync(taskImagesIds, ImageSource.Project, errors);
      List<ImageInfo> userImagesInfo = await GetImagesAsync(
        usersDatas?.Where(u => u.ImageId.HasValue).Select(u => u.ImageId.Value).ToList(), ImageSource.User, errors);

      return new OperationResultResponse<TaskResponse>()
      {
        Status = errors.Any() ? OperationResultStatusType.PartialSuccess : OperationResultStatusType.FullSuccess,
        Body = _taskResponseMapper.Map(
          dbTask, project, usersDatas, taskImagesInfo, userImagesInfo),
        Errors = errors
      };
    }
  }
}
