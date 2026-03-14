using System;
using JobService.API.Models;

namespace JobService.API.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(Job job);
}
