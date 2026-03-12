using System;

namespace JobService.API.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
