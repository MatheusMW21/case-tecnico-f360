using JobService.API.Interfaces;
using JobService.API.Models;

namespace JobService.API.Handlers;

public class EnviarEmailHandler : IJobHandler
{
    private readonly ILogger<EnviarEmailHandler> _logger;
    public EnviarEmailHandler(ILogger<EnviarEmailHandler> logger)
    {
        _logger = logger;      
    }

    public string JobType => "EnviarEmail"; 

    public Task HandleAsync(Job job)
    {
        _logger.LogInformation("Processando envio de e-mail para o Job: {JobId}", job.Id);
        return Task.CompletedTask;
    }
}
