using MemDrawer.Infrastructure.Database;
using MemDrawer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemDrawer.Infrastructure;

public static class Extensions
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddSqliteDbContext<AppDbContext>("db");
        builder.Services.AddTransient<IResponseBuilder, ResponseBuilder>();
        builder.Services.AddTransient<IImageValidator, ImageValidator>();
        
        ImageSharpTextDrawer.Init();
    }
}