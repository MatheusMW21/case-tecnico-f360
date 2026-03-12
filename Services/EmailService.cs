using System;
using JobService.API.Interfaces;

namespace JobService.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    public EmailService (ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation($"Enviando email para: {to}");
        return Task.CompletedTask;
    }
}
