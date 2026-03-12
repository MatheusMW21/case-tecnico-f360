using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JobService.API.Models;

public class Task
{
    public int Id { get; set; }
    public required String TaskType { get; set; }

    [JsonPropertyName("processing_data")]
    public required String ProcessingData { get; set; }
}
