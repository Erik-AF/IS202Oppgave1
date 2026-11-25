// Starter MariaDB og webapplikasjonen gjennom Aspire.
var builder = DistributedApplication.CreateBuilder(args);

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
    .WithVolume("mariadb-data", "/var/lib/mysql")
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddDockerfile(
    "Oppgave202",
    "..",
    "202Oppgave/Dockerfile")
    .WithHttpEndpoint(
        port: 5027,
        targetPort: 8080)

    // Bevarer opplastede bilder når webcontaineren erstattes.
    .WithVolume("ressursbilder", "/app/wwwroot/uploads");

builder.Build().Run();
