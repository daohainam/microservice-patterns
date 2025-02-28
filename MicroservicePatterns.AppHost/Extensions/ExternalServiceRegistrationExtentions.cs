﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspire.Hosting;

namespace MicroservicePatterns.AppHost.Extensions;
public static class ExternalServiceRegistrationExtentions
{
    public static IDistributedApplicationBuilder AddApplicationServices(this IDistributedApplicationBuilder builder)
    {
        var cache = builder.AddRedis("redis").WithRedisInsight();
        var kafka = builder.AddKafka("kafka").WithKafkaUI();
        var mongoDb = builder.AddMongoDB("mongodb").WithMongoExpress().WithDataVolume(); // here we use MongoDB for both read/write model, but we can use different databases using replicas
        var postgres = builder.AddPostgres("postgresql").WithPgWeb().WithDataVolume();
        
        var borrowerDb = postgres.AddDatabase("cqrs-borrower");
        var borrowerApi = builder.AddProject<Projects.CQRS_Library_BorrowerApi>("cqrs-borrower-api")
            .WithReference(kafka)
            .WithReference(borrowerDb);

        return builder;
    }
}
