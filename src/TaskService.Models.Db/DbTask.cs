using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.TaskService.Models.Db
{
  public class DbTask
  {
    public const string TableName = "Tasks";

    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid ProjectId { get; set; }
    public string Description { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid TypeId { get; set; }
    public Guid StatusId { get; set; }
    public Guid PriorityId { get; set; }
    public int? PlannedMinutes { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public int Number { get; set; }

    public DbTaskProperty Status { get; set; }
    public DbTaskProperty Priority { get; set; }
    public DbTaskProperty Type { get; set; }

    public DbTask Parent { get; set; }

    public ICollection<DbTask> Subtasks { get; set; }
    public ICollection<DbTaskImage> Images { get; set; }

    public DbTask()
    {
      Subtasks = new HashSet<DbTask>();
      Images = new HashSet<DbTaskImage>();
    }
  }

  public class DbTaskConfiguration : IEntityTypeConfiguration<DbTask>
  {
    public void Configure(EntityTypeBuilder<DbTask> builder)
    {
      builder
       .ToTable(DbTask.TableName);

      builder
        .HasKey(t => t.Id);

      builder
        .Property(t => t.Name)
        .IsRequired();

      builder
        .HasOne(t => t.Status)
        .WithMany(tp => tp.StatusTasks);

      builder
        .HasOne(t => t.Type)
        .WithMany(tp => tp.TypeTasks);

      builder
        .HasOne(t => t.Priority)
        .WithMany(tp => tp.PriorityTasks);

      builder
        .HasOne(t => t.Parent)
        .WithMany(t => t.Subtasks);

      builder
        .HasMany(t => t.Subtasks)
        .WithOne(tp => tp.Parent);

      builder
        .HasMany(p => p.Images)
        .WithOne(tp => tp.Task);
    }
  }
}
