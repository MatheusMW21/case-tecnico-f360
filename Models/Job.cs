namespace JobService.API.Models;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required String TaskType { get; set; }
    public required String Payload { get; set; }
    public JobStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } 
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public String? ErrorMessage { get; set; }
}

public enum JobStatus {
    Pendente,
    EmProcessamento,
    Concluido,
    Erro
}
