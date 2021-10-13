using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LT.DigitalOffice.TaskService.Models.Dto.Enums
{
  [JsonConverter(typeof(StringEnumConverter))]
  public enum TaskPropertyType
  {
    Type,
    Status,
    Priority
  }
}
