namespace JobService.API.DTOs;

public class CreateJobRequest
{   
    public required string TaskType { get; set; }
    public required string Payload { get; set; }
}   
