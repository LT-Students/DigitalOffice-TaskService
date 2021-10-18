using LT.DigitalOffice.TaskService.Mappers.PatchDocument.Interfaces;
using LT.DigitalOffice.TaskService.Models.Db;
using LT.DigitalOffice.TaskService.Models.Dto.Requests;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace LT.DigitalOffice.TaskService.Mappers.PatchDocument
{
  public class PatchDbTaskMapper : IPatchDbTaskMapper
  {
    public JsonPatchDocument<DbTask> Map(JsonPatchDocument<EditTaskRequest> request)
    {
      if (request == null)
      {
        return null;
      }

      var result = new JsonPatchDocument<DbTask>();

      foreach (var item in request.Operations)
      {
        result.Operations.Add(new Operation<DbTask>(item.op, item.path, item.from, item.value));
      }

      return result;
    }
  }
}
