using EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.Kafka;
public static class KafkaEventBusExtensions
{
    public static IHostApplicationBuilder AddKafkaEventPublisher(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.AddKafkaProducer<string, string>(connectionName);

        return builder;
    }
}

