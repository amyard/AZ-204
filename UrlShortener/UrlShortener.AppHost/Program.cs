var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("url-shortener");

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.UrlShortener_Api>("urlshortener-api")
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

builder.Build().Run();
