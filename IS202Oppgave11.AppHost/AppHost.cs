var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerfile(
    "Oppgave202",
    "..",
    "202Oppgave/Dockerfile")
    .WithHttpEndpoint(
        port: 5027,
        targetPort: 8080);

builder.Build().Run();
