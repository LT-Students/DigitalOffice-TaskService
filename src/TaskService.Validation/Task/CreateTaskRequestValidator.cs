using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Models.Broker.Common;
using LT.DigitalOffice.Models.Broker.Requests.Project;
using LT.DigitalOffice.ProjectService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Data.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using LT.DigitalOffice.TaskService.Validation.Image.Interfaces;
using LT.DigitalOffice.TaskService.Validation.Task.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LT.DigitalOffice.TaskService.Validation.Task
{
  public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>, ICreateTaskRequestValidator
  {
    private readonly IRequestClient<ICheckProjectsExistence> _rcCheckProjects;
    private readonly IRequestClient<ICheckProjectUsersExistenceRequest> _rcCheckProjectUsers;
    private readonly ILogger<CreateTaskRequestValidator> _logger;

    private async Task<bool> DoesProjectExist(Guid projectId)
    {
      var logMessage = "Cannot check projects existence.";

      try
      {
        var response =
          await _rcCheckProjects.GetResponse<IOperationResult<ICheckProjectsExistence>>(
            ICheckProjectsExistence.CreateObj(new() { projectId }));

        if (response.Message.IsSuccess)
        {
          return response.Message.Body.ProjectsIds?.Any() ?? false;
        }

        _logger.LogWarning(logMessage);
      }
      catch (Exception exc)
      {
        _logger.LogError(exc, logMessage);
      }

      return false;
    }

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

    public CreateTaskRequestValidator(
      ITaskRepository tasksRepository,
      ITaskPropertyRepository taskPropertyRepository,
      IImageValidator imageValidator,
      IRequestClient<ICheckProjectsExistence> rcCheckProjects,
      IRequestClient<ICheckProjectUsersExistenceRequest> rcCheckProjectsUsers,
      ILogger<CreateTaskRequestValidator> logger)
    {
      _rcCheckProjects = rcCheckProjects;
      _rcCheckProjectUsers = rcCheckProjectsUsers;
      _logger = logger;

      List<string> errors = new();

      RuleFor(task => task.Name)
        .NotEmpty()
        .MaximumLength(150)
        .WithMessage("Task name is too long.");

      When(task => !string.IsNullOrEmpty(task.Description?.Trim()), () =>
      {
        RuleFor(task => task.Description)
          .MaximumLength(300)
          .WithMessage("Task description is too long.");
      });

      When(task => task.ParentId.HasValue, () =>
      {
        DbTask parentTask = null;

        RuleFor(task => task.ParentId)
          .Must(x =>
          {
            parentTask = tasksRepository.Get(x.Value, false);
            return parentTask != null;
          })
          .WithMessage("Task does not exist.")
          .Must(_ =>
          {
            return parentTask?.ParentId == null;
          })
          .WithMessage("Parent task must have not to have a parent.");
      });

      When(project => project.TaskImages != null && project.TaskImages.Any(), () =>
      {
        RuleForEach(project => project.TaskImages)
          .SetValidator(imageValidator);
      });

      When(task => task.AssignedTo.HasValue, () =>
      {
        RuleFor(task => task)
          .MustAsync(async (task, _) => await DoesProjectUserExist(task.ProjectId, task.AssignedTo.Value))
          .WithMessage("User does not exist.");
      });

      RuleFor(task => task.ProjectId)
        .NotEmpty()
        .MustAsync(async (x, _) => await DoesProjectExist(x))
        .WithMessage("Project does not exist.");

      RuleFor(task => task)
        .NotEmpty()
        .Must(x => taskPropertyRepository.DoesExist(x.PriorityId, TaskPropertyType.Priority))
        .WithMessage("Priority id does not exist.");

      RuleFor(task => task)
        .NotEmpty()
        .Must(x => taskPropertyRepository.DoesExist(x.StatusId, TaskPropertyType.Status))
        .WithMessage("Status id does not exist.");

      RuleFor(task => task)
        .NotEmpty()
        .Must(x => taskPropertyRepository.DoesExist(x.TypeId, TaskPropertyType.Type))
        .WithMessage("Type id does not exist.");
    }
  }
}
