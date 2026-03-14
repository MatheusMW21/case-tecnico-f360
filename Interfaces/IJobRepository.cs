using JobService.API.Models;

namespace JobService.API.Interfaces;

public interface IJobRepository
{
    Task InsertAsync(Job job);
    Task<Job?> GetByIdAsync(Guid id);
    Task UpdateStatusAsync(Job job);
    Task<IEnumerable<Job>> GetPendingJobsAsync();
}
