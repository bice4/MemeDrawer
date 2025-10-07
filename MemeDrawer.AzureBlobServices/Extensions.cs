using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MemeDrawer.AzureBlobServices;

public static class Extensions
{
    public static void AddAzureBlobServices(this IHostApplicationBuilder builder,
        string connectionName = "imagecontainer")
    {
        builder.AddAzureBlobServiceClient(connectionName);

        builder.Services.AddSingleton<IAzureBlobService, AzureBlobService>();
    }
}