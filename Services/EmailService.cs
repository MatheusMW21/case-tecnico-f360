using System;
using JobService.API.Interfaces;

namespace JobService.API.Services;

public class EmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        Console.WriteLine($"Enviando email para: {to}");
        return Task.CompletedTask;
    }
}
