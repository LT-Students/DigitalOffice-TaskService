using System;
using LT.DigitalOffice.TaskService.Mappers.PatchDocument.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Enums;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TaskService.Mappers.PatchDocument
{
  public class PatchDbTaskPropertyMapper : IPatchDbTaskPropertyMapper
  {
    public JsonPatchDocument<DbTaskProperty> Map(JsonPatchDocument<EditTaskPropertyRequest> request)
    {
      if (request == null)
      {
        return null;
      }

      var result = new JsonPatchDocument<DbTaskProperty>();

      foreach (var item in request.Operations)
      {
        if (item.path.EndsWith(nameof(EditTaskPropertyRequest.PropertyType), StringComparison.OrdinalIgnoreCase))
        {
          result.Operations.Add(new Operation<DbTaskProperty>(item.op, item.path, item.from, (int)Enum.Parse(typeof(TaskPropertyType), item.value.ToString())));
          continue;
        }

        else
        {
          result.Operations.Add(new Operation<DbTaskProperty>(item.op, item.path, item.from, item.value));
        }
      }

      return result;
    }
  }
}
