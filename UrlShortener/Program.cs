using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using Prometheus;
using StackExchange.Redis;
using UrlShortener;
using UrlShortener.Application;
using UrlShortener.Application.Models;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra;
using UrlShortener.Infra.BackgroundJobs;
using UrlShortener.Infra.Configurations;
using UrlShortener.Infra.Context;

var builder = WebApplication.CreateBuilder(args);


builder.AddBackgroundJobs();
builder.AddDatabase();
builder.AddServices();

var appSettings = builder.Configuration.GetRequiredSection("ApplicationSettings").Get<ApplicationSettings>();
builder.Services.AddSingleton(appSettings!);

var app = builder.Build();

app.UseObservabilityServices();
app.UseEntryPoint();
app.MapEndpoints();



app.Run();

public class Startup
{
}