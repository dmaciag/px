using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PxServices.CoreAlgos;
using PxServices.Interfaces;
using PxServices.Repositories;
using PxServices.Services;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//algo's
builder.Services.AddScoped<IJttSeriesAlgo, JttSeriesAlgo>();
builder.Services.AddScoped<ITickerInfoAlgo, TickerInfoAlgo>();
builder.Services.AddScoped<IHistoricRatioAlgo, HistoricRatioAlgo>();
builder.Services.AddScoped<IPhaseSeriesAlgo, PhaseSeriesAlgo>(); 
builder.Services.AddScoped<IPinBarAlgo, PinBarAlgo>();

//services
builder.Services.AddScoped<IEngine, Engine>();
builder.Services.AddScoped<IDataRetrievalService, DataRetrievalService>();

//repo's
builder.Services.AddScoped<IPhaseSeriesRepository, PhaseSeriesRepository>();
builder.Services.AddScoped<IPinBarRepository, PinBarRepository>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {

        options.AddDefaultPolicy(
            policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
    });
}

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
 