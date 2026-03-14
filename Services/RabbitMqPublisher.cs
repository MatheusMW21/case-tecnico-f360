using System.Text.Json;
using JobService.API.Interfaces;
using JobService.API.Models;
using RabbitMQ.Client;

namespace JobService.API.Services;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _queueName;
    public RabbitMqPublisher (IConfiguration configuration)
    {
        _host = configuration["RabbitMQ:Host"] ?? string.Empty; 
        _port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"); 
        _username = configuration["RabbitMQ:Username"] ?? string.Empty; 
        _password = configuration["RabbitMQ:Password"] ?? string.Empty; 
        _queueName = configuration["RabbitMQ:QueueName"] ?? string.Empty; 
    }
    public async Task PublishAsync(Job job)
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
        
        var body = JsonSerializer.SerializeToUtf8Bytes(job);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            body: body
        );
    }
}
