using Newtonsoft.Json;

namespace Schedule.ImageService.Models
{
    public class Group
    {
        [JsonProperty("group_id")] public long GroupId { get; set; }

        [JsonProperty("group_name")] public string GroupName { get; set; }
    }

    public sealed class KpfuGroupRoot
    {
        [JsonProperty("group_list")] public Group[] Groups { get; set; }
    }
}