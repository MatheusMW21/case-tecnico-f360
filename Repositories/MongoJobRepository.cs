using JobService.API.Interfaces;
using JobService.API.Models;
using MongoDB.Driver;

namespace JobService.API.Repositories;

public class MongoJobRepository : IJobRepository
{
    private readonly IMongoCollection<Job> _collection;

    public MongoJobRepository(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"];
        var databaseName = configuration["MongoDB:DatabaseName"];
        var collectionName = configuration["MongoDB:CollectionName"];

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<Job>(collectionName);
    }

    public async Task InsertAsync(Job job)
    {
        await _collection.InsertOneAsync(job);
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(Builders<Job>.Filter.Eq(j => j.Id, id)).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Job>> GetPendingJobsAsync()
    {
        return await _collection.Find(j => j.Status == JobStatus.Pendente).ToListAsync();
    }


    public async Task UpdateStatusAsync(Job job)
    {
        var filter = Builders<Job>.Filter.Eq(j => j.Id, job.Id);
        await _collection.ReplaceOneAsync(filter, job);
    }
}
