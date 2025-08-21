using UrlShortener.Application;
using UrlShortener.Infra;
using UrlShortener.Infra.Configurations;

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