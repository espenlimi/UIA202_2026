using Heimevernet.Aspire.AppHost.MariaDb;

var builder = DistributedApplication.CreateBuilder(args);

var mariaDbServer = builder.AddMariaDb("mariadb")
                   .WithDataBindMount(source: @"../../../MariaDb/Data")
                   .WithLifetime(ContainerLifetime.Persistent);

var mariaDb = mariaDbServer.AddDatabase("heimevernetdb");

//Bruk enten dockerfile varianten eller native, ikke begge 

//Variant dockerfile
//builder.AddDockerfile("heimevernet-web", "../../", "Heimevernet.Web/Dockerfile")
//                       .WithExternalHttpEndpoints()
//                       .WithReference(mariaDb)
//                       .WaitFor(mariaDb)
//                       .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "heimevernet-web");

//Det tar en time å gå ned til ørsta rådhus!

//Variant native 
builder.AddProject<Projects.Heimevernet_Web>("heimevernet-web")
                       .WithReference(mariaDb)
                       .WaitFor(mariaDb);
builder.Build().Run();
