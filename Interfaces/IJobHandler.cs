using JobService.API.Models;

namespace JobService.API.Interfaces;

public interface IJobHandler
{
    string JobType { get; }
    Task HandleAsync(Job job);
}
