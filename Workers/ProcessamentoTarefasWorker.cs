using System.Text.Json;
using JobService.API.Interfaces;
using JobService.API.Models;
using JobService.API.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace JobService.API.Workers;

public class ProcessamentoTarefasWorker : BackgroundService
{
    private readonly ILogger<ProcessamentoTarefasWorker> _logger;
    private readonly IServiceProvider _provider;
     private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _queueName;
    public ProcessamentoTarefasWorker(ILogger<ProcessamentoTarefasWorker> logger, IServiceProvider provider, IConfiguration configuration)
    {
        _host = configuration["RabbitMQ:Host"] ?? string.Empty; 
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"); 
        _username = configuration["RabbitMQ:Username"] ?? string.Empty; 
        _password = configuration["RabbitMQ:Password"] ?? string.Empty; 
        _queueName = configuration["RabbitMQ:QueueName"] ?? string.Empty; 

        _logger = logger;
        _provider = provider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { Port = _port, HostName = _host, UserName = _username, Password = _password};
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        
        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false
        );
 
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, eventArgs) =>
        {
            using var scope = _provider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
            var resolver = scope.ServiceProvider.GetRequiredService<JobHandlerResolver>();
            var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
            var job = JsonSerializer.Deserialize<Job>(eventArgs.Body.ToArray());
            
            if(job is null)
            {
                _logger.LogError("Falha ao processar mensagem: O corpo da mensagem não pôde ser convertido para um Job.");
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false); 
                return;
            }

            job.Status = JobStatus.EmProcessamento;
            job.UpdatedAt = DateTimeOffset.UtcNow; 
            await repository.UpdateStatusAsync(job);

            var handler = resolver.Resolve(job.TaskType);

            if (handler is null)
            {
                job.Status = JobStatus.Erro;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                await repository.UpdateStatusAsync(job);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false); 
                return;
            }

            try
            {
                await handler.HandleAsync(job);

                job.Status = JobStatus.Concluido;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                await repository.UpdateStatusAsync(job);

                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);

            } catch (Exception ex)
            {
                job.AttemptCount++;
                job.ErrorMessage = ex.Message;
                job.UpdatedAt = DateTimeOffset.UtcNow;

                if (job.AttemptCount < job.MaxAttempts)
                {
                    job.Status = JobStatus.Pendente;
                    await repository.UpdateStatusAsync(job);
                    await messagePublisher.PublishAsync(job);

                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false); 
                    _logger.LogWarning("Job {JobId} falhou. Tentativa {AttemptCount}/{MaxAttempts}. Republicando...", job.Id, job.AttemptCount, job.MaxAttempts);
                } else
                {
                    job.Status = JobStatus.Erro;
                    await repository.UpdateStatusAsync(job);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, false); 
                    _logger.LogError("Job {JobId} falhou após {AttemptCount} tentativas. Erro: {Message}", job.Id, job.AttemptCount, ex.Message);
                }
            }
        };
        
        await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
