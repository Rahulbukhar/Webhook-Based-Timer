using DatasetService.Core.Repository.Azure.CosmosDB;
using WebhookBasedTimer.Repository;
using WebhookBasedTimer.Service;
using NoSqlDataAccess.Azure;
using WebhookBasedTimer.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITimerService, TimerService>();
builder.Services.AddSingleton<ITimerRepository, TimerRepository>();
builder.Services.AddSingleton<IConfigProvider, ConfigProvider>();
builder.Services.AddSingleton<IWebhookService, WebhookService>();
builder.Services.ConfigureCosmosDb<CosmosDbConnection, CosmosDbProvisioningConfiguration>();
builder.Services.AddHostedService<TimerBackgroundService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
