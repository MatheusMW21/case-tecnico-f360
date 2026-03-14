using JobService.API.DTOs;
using JobService.API.Interfaces;
using JobService.API.Models;
using JobService.API.Repositories;
using JobService.API.Services;
using JobService.API.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ProcessamentoTarefasWorker>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJobRepository, MongoJobRepository>();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/jobs", async (CreateJobRequest request, IJobRepository repository) => {
    var job = new Job
    {
        TaskType = request.TaskType,
        Payload = request.Payload
    };
    
    await repository.InsertAsync(job);
    return Results.Created($"/jobs/{job.Id}", job); 
});

app.MapGet("/jobs/{id}", async (Guid id, IJobRepository repository) =>
{
    var job = await repository.GetByIdAsync(id);

    if (job == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(job);
}); 

app.Run();