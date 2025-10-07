var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqlite("db")
    .WithSqliteWeb();

var azureBlobStorage = builder.AddAzureStorage("azureblobstorage")
    .RunAsEmulator(emulator =>
    {
        emulator.WithDataVolume();
        emulator.WithLifetime(ContainerLifetime.Persistent);
    });

var azureBlobPhotoContainer = azureBlobStorage.AddBlobs("imagecontainer");

var apiService = builder.AddProject<Projects.MemDrawer_ApiService>("apiservice")
    .WithReference(azureBlobPhotoContainer)
    .WithReference(db)
    .WaitFor(db)
    .WaitFor(azureBlobPhotoContainer)
    .WithHttpHealthCheck("/health");

builder.AddViteApp(name: "frontend", workingDirectory: "../mem-drawer-frontend")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithNpmPackageInstallation();

builder.Build().Run();