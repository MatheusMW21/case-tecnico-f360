using JobService.API.Interfaces;

namespace JobService.API.Services;

public class JobHandlerResolver
{
    private readonly IEnumerable<IJobHandler> _handlers;
    public JobHandlerResolver(IEnumerable<IJobHandler> handlers)
    {
        _handlers = handlers;
    }

    public IJobHandler? Resolve(string taskType)
    {
        return _handlers.FirstOrDefault(h => h.JobType == taskType);
    }
}
