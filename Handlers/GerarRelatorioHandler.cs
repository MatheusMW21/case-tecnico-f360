using JobService.API.Interfaces;
using JobService.API.Models;

namespace JobService.API.Handlers;

public class GerarRelatorioHandler : IJobHandler
{
    private readonly ILogger<GerarRelatorioHandler> _logger;
    public GerarRelatorioHandler(ILogger<GerarRelatorioHandler> logger)
    {
        _logger = logger;
    }

    public string JobType => "GerarRelatorio";

    public Task HandleAsync(Job job)
    {
        _logger.LogInformation("Processando envio de relatório para o Job: {JobId}", job.Id);
        return Task.CompletedTask;
    }
}
