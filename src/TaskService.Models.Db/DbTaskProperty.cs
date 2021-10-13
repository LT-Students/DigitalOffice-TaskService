using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.TaskService.Models.Db
{
  public class DbTaskProperty
  {
    public const string TableName = "TaskProperties";

    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ProjectId { get; set; }
    public int PropertyType { get; set; }
    public string Description { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
    public bool IsActive { get; set; }

    public ICollection<DbTask> PriorityTasks { get; set; }
    public ICollection<DbTask> TypeTasks { get; set; }
    public ICollection<DbTask> StatusTasks { get; set; }
  }

  public class DbTaskPropertyConfiguration : IEntityTypeConfiguration<DbTaskProperty>
  {
    public void Configure(EntityTypeBuilder<DbTaskProperty> builder)
    {
      builder
        .ToTable(DbTaskProperty.TableName);

      builder
        .HasKey(tp => tp.Id);

      builder
        .Property(tp => tp.Name)
        .IsRequired();

      builder
        .HasMany(tp => tp.PriorityTasks)
        .WithOne(T => T.Priority);

      builder
        .HasMany(tp => tp.TypeTasks)
        .WithOne(T => T.Type);

      builder
        .HasMany(tp => tp.StatusTasks)
        .WithOne(T => T.Status);
    }
  }
}
