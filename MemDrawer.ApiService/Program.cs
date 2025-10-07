using MemDrawer.ApiService.Services;
using MemDrawer.Infrastructure;
using MemDrawer.Infrastructure.Database;
using MemDrawer.ServiceDefaults;
using MemeDrawer.AzureBlobServices;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddInfrastructure();
builder.AddAzureBlobServices();
builder.Services.AddResponseCaching();

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<IImageService, ImageService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.MapDefaultEndpoints();
app.UseResponseCaching();

app.MigrateAndSeed();
app.Run();