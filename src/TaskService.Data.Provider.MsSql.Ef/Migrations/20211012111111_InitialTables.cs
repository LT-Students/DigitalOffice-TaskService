using System;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LT.DigitalOffice.TaskService.Data.Provider.MsSql.Ef.Migrations
{
  [DbContext(typeof(TaskServiceDbContext))]
  [Migration("20211012111111_InitialTables")]
  public class InitialTables : Migration
  {
    private void AddTasksTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbTask.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          Name = table.Column<string>(nullable: false),
          ProjectId = table.Column<Guid>(nullable: false),
          Description = table.Column<string>(nullable: true),
          AssignedTo = table.Column<Guid>(nullable: true),
          TypeId = table.Column<Guid>(nullable: false),
          StatusId = table.Column<Guid>(nullable: false),
          PriorityId = table.Column<Guid>(nullable: false),
          PlannedMinutes = table.Column<int>(nullable: true),
          ParentId = table.Column<Guid>(nullable: true),
          CreatedBy = table.Column<Guid>(nullable: false),
          CreatedAtUtc = table.Column<DateTime>(nullable: false),
          ModifiedBy = table.Column<Guid>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true),
          Number = table.Column<int>(nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Tasks", x => x.Id);
        });
    }

    private void AddTaskPropertiesTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbTaskProperty.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          Name = table.Column<string>(nullable: false),
          ProjectId = table.Column<Guid>(nullable: true),
          PropertyType = table.Column<int>(nullable: false),
          Description = table.Column<string>(nullable: true),
          CreatedBy = table.Column<Guid>(nullable: true),
          CreatedAtUtc = table.Column<DateTime>(nullable: true),
          ModifiedBy = table.Column<Guid>(nullable: true),
          ModifiedAtUtc = table.Column<DateTime>(nullable: true),
          IsActive = table.Column<bool>(nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TaskProperties", x => x.Id);
        });
    }

    private void AddTaskImagesTable(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: DbTaskImage.TableName,
        columns: table => new
        {
          Id = table.Column<Guid>(nullable: false),
          TaskId = table.Column<Guid>(nullable: false),
          ImageId = table.Column<Guid>(nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TaskImages", x => x.Id);
        });
    }

    private void AddProperty(
      MigrationBuilder migrationBuilder,
      string name,
      int type)
    {
      migrationBuilder.InsertData(
        table: DbTaskProperty.TableName,
        columns: new[]
        {
          nameof(DbTaskProperty.Id),
          nameof(DbTaskProperty.Name),
          nameof(DbTaskProperty.ProjectId),
          nameof(DbTaskProperty.CreatedBy),
          nameof(DbTaskProperty.PropertyType),
          nameof(DbTaskProperty.Description),
          nameof(DbTaskProperty.CreatedAtUtc),
          nameof(DbTaskProperty.IsActive)
        },
        columnTypes: new[]
        {
          "uniqueidentifier",
          "nvarchar(max)",
          "uniqueidentifier",
          "uniqueidentifier",
          "int",
          "nvarchar(max)",
          "datetime2",
          "bit"
        },
        values: new object[]
        {
          Guid.NewGuid(),
          name,
          null,
          null,
          type,
          null,
          null,
          true
        });
    }

    private void AddDefaultTypes(MigrationBuilder migrationBuilder)
    {
      AddProperty(migrationBuilder, "Feature", (int)TaskPropertyType.Type);
      AddProperty(migrationBuilder, "Bug", (int)TaskPropertyType.Type);
      AddProperty(migrationBuilder, "Task", (int)TaskPropertyType.Type);
    }

    private void AddDefaultPriorities(MigrationBuilder migrationBuilder)
    {
      AddProperty(migrationBuilder, "Normal", (int)TaskPropertyType.Priority);
      AddProperty(migrationBuilder, "High", (int)TaskPropertyType.Priority);
      AddProperty(migrationBuilder, "Low", (int)TaskPropertyType.Priority);
    }

    private void AddDefaultStatuses(MigrationBuilder migrationBuilder)
    {
      AddProperty(migrationBuilder, "New", (int)TaskPropertyType.Status);
      AddProperty(migrationBuilder, "In Progress", (int)TaskPropertyType.Status);
      AddProperty(migrationBuilder, "Done", (int)TaskPropertyType.Status);
    }

    protected override void Up(MigrationBuilder migrationBuilder)
    {
      AddTasksTable(migrationBuilder);

      AddTaskPropertiesTable(migrationBuilder);

      AddTaskImagesTable(migrationBuilder);

      AddDefaultTypes(migrationBuilder);

      AddDefaultPriorities(migrationBuilder);

      AddDefaultStatuses(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(DbTask.TableName);
      migrationBuilder.DropTable(DbTaskProperty.TableName);
    }
  }
}
