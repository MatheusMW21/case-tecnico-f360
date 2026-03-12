using System;
using JobService.API.Interfaces;

namespace JobService.API.Workers;

public class ProcessamentoTarefasWorker : BackgroundService
{
    private readonly ILogger<ProcessamentoTarefasWorker> _logger;
    private readonly IServiceProvider _provider;
    public ProcessamentoTarefasWorker(ILogger<ProcessamentoTarefasWorker> logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation($"Processando tarefas - {DateTime.Now.TimeOfDay}");
            
            using var scope = _provider.CreateScope();
            
            var service = scope.ServiceProvider.GetRequiredService<IEmailService>();

            await service.SendEmailAsync("teste@email.com", "Teste worker", "Olá, Estou testando a tarefa de enviar email! Atenciosamente");
        }
    }
}
