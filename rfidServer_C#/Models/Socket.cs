using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace rfidServer_C_.Models;

public class Socket
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class MessageData
{
    [JsonPropertyName("to")]
    public string? To { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class QueryData
{
    [JsonPropertyName("tagId")]
    public string? TagID { get; set; }
}

public class HistoryData
{
    [JsonPropertyName("rid")]
    public Guid? Rid { get; set; }

    [JsonPropertyName("tagId")]
    public string? TagID { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("action")]
    public string? Action { get; set; }

    [JsonPropertyName("bDate")]
    public string? BDate { get; set; }

    [JsonPropertyName("bUser")]
    public string? BUser { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }


}
