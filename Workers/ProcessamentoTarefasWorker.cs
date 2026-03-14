using System.Text.Json;
using JobService.API.Interfaces;
using JobService.API.Models;
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
            var job = JsonSerializer.Deserialize<Job>(eventArgs.Body.ToArray());
            
            if(job is null)
            {
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false); 
                return;
            }

            job.Status = JobStatus.EmProcessamento;
            job.UpdatedAt = DateTimeOffset.UtcNow; 
            await repository.UpdateStatusAsync(job);

            _logger.LogInformation($"A tarefa: {job.TaskType}. Está em processamento.");

            job.Status = JobStatus.Concluido;
            job.UpdatedAt = DateTimeOffset.UtcNow;
            await repository.UpdateStatusAsync(job);

            _logger.LogInformation($"{job.TaskType}, concluído com sucesso");

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        };
        
        await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
