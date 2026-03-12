using JobService.API.Interfaces;
using JobService.API.Repositories;
using JobService.API.Services;
using JobService.API.Workers;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddHostedService<ProcessamentoTarefasWorker>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJobRepository, MongoJobRepository>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();