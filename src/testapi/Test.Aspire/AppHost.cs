using Scalar.Aspire;
using Test.Aspire.Extensions;

var builder = DistributedApplication.CreateBuilder(args);
var mongo = builder.AddMongoDB("MongoDb", userName: builder.GetSaUser(), password: builder.GetSaPass());
var mongoPort = mongo.Resource.Port;
var dbGate = builder.AddContainer("DbGate", "dbgate/dbgate:latest");
dbGate.WithEndpoint("web", e =>
{
    e.Port = 8081;
    e.TargetPort = 3000;
    e.UriScheme = "http";
});
dbGate.WithEnvironment("ENGINE_mongo", "mongo@dbgate-plugin-mongo")
.WithEnvironment("SERVER_mongo", "MongoDb")
.WithEnvironment("PORT_mongo", mongoPort)
.WithEnvironment("USER_mongo", builder.GetSaUser())
.WithEnvironment("PASSWORD_mongo", builder.GetSaPass())
.WithEnvironment("CONNECTIONS", "mongo")
.WaitFor(mongo);

var kafka = builder.AddKafka("Kafka").WithKafkaUI();

var api = builder.AddProject<Projects.Test_Api>("Api")
.WithReference(mongo).WaitFor(mongo)
.WithReference(kafka).WaitFor(kafka);

// Add API Reference
var scalar = builder.AddScalarApiReference()
.WithApiReference(api);

builder.Build().Run();