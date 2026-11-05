var builder = DistributedApplication.CreateBuilder(args);

// MariaDB-container
var mariadb = builder.AddContainer(
        "mariadb",
        "mariadb",
        "11")
    .WithEnvironment("MARIADB_ROOT_PASSWORD", "rootpassord")
    .WithEnvironment("MARIADB_DATABASE", "resourcesdb")
    .WithEnvironment("MARIADB_USER", "appuser")
    .WithEnvironment("MARIADB_PASSWORD", "erikerkul")
    .WithEndpoint(
        targetPort: 3306,
        name: "mysql")
    .WithLifetime(ContainerLifetime.Persistent);

// MVC-applikasjon
builder.AddDockerfile(
    "Oppgave202",
    "..",
    "202Oppgave/Dockerfile")
    .WithHttpEndpoint(
        port: 5027,
        targetPort: 8080)
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();
