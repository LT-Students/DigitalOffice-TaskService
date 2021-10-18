using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LT.DigitalOffice.TaskService.Models.Db
{
  public class DbTaskImage
  {
    public const string TableName = "EntitiesImages";

    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid ImageId { get; set; }

    public DbTask Task { get; set; }
  }

  public class DbProjectImageConfiguration : IEntityTypeConfiguration<DbTaskImage>
  {
    public void Configure(EntityTypeBuilder<DbTaskImage> builder)
    {
      builder
        .ToTable(DbTaskImage.TableName);

      builder
        .HasKey(p => p.Id);

      builder
        .HasOne(pu => pu.Task)
        .WithMany(p => p.Images);
    }
  }
}
