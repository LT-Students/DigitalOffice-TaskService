using System;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.TaskService.Mappers.Db.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.Http;

namespace LT.DigitalOffice.TaskService.Mappers.Db
{
  public class DbTaskPropertyMapper : IDbTaskPropertyMapper
  {
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DbTaskPropertyMapper(IHttpContextAccessor httpContextAccessor)
    {
      _httpContextAccessor = httpContextAccessor;
    }

    public DbTaskProperty Map(CreateTaskPropertyRequest request)
    {
      if (request == null)
      {
        return null;
      }

      return new DbTaskProperty
      {
        Id = Guid.NewGuid(),
        ProjectId = request.ProjectId,
        Name = request.Name,
        IsActive = true,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = _httpContextAccessor.HttpContext.GetUserId(),
        Description = string.IsNullOrEmpty(request.Description?.Trim()) ? null : request.Description.Trim(),
        PropertyType = (int)request.PropertyType,
      };
    }
  }
}
