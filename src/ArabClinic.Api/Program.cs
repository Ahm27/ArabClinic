using ArabClinic.Application;
using ArabClinic.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new
{
    service = "ArabClinic.Api",
    status = "running"
}));

app.Run();

public partial class Program;