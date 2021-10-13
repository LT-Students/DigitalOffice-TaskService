using System;
using System.Collections.Generic;
using System.Linq;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TaskService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TaskService.Mappers.Helpers;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TaskService.Mappers.Db
{
  public class DbTaskMapper : IDbTaskMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbTaskImageMapper _dbTaskImageMapper;

    public DbTaskMapper(
      IHttpContextAccessor httpContextAccessor,
      IDbTaskImageMapper dbTaskImageMapper)
    {
      _httpContextAccessor = httpContextAccessor;
      _dbTaskImageMapper = dbTaskImageMapper;
    }

    public DbTask Map(CreateTaskRequest taskRequest, List<Guid> imagesIds)
    {
      if (taskRequest == null)
      {
        return null;
      }

      var taskId = Guid.NewGuid();

      return new DbTask
      {
        Id = taskId,
        Name = taskRequest.Name,
        Description = string.IsNullOrEmpty(taskRequest.Description?.Trim()) ? null : taskRequest.Description.Trim(),
        PlannedMinutes = taskRequest.PlannedMinutes,
        AssignedTo = taskRequest.AssignedTo,
        ProjectId = taskRequest.ProjectId,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        ParentId = taskRequest.ParentId,
        PriorityId = taskRequest.PriorityId,
        StatusId = taskRequest.StatusId,
        TypeId = taskRequest.TypeId,
        Number = TaskNumberHelper.GetProjectTaskNumber(taskRequest.ProjectId),
        Images = imagesIds?.Select(imageId => _dbTaskImageMapper.Map(taskId, imageId)).ToList()
      };
    }
  }
}
