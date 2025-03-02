using Confluent.Kafka;
using EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus.Kafka;
public static class KafkaEventBusExtensions
{
    public static IHostApplicationBuilder AddKafkaEventPublisher(this IHostApplicationBuilder builder, string connectionName)
    {
        builder.AddKafkaProducer<string, MessageEnvelop>(connectionName,
            configureSettings: (settings) => { 
            },
            configureBuilder: (builder) =>
            {
                builder.SetValueSerializer(new MessageEnvelopSerializer());
            }
            );

        return builder;
    }

    public static void AddKafkaEventPublisher(this IServiceCollection services, string topic)
    {
        services.AddTransient<IEventPublisher>(services => new KafkaEventPublisher(
            topic,
            services.GetRequiredService<IProducer<string, MessageEnvelop>>(),
            services.GetRequiredService<ILogger<KafkaEventPublisher>>()
            ));
    }

    public static IHostApplicationBuilder AddKafkaMessageEnvelopConsumer(this IHostApplicationBuilder builder, string groupId, string connectionName = "kafka")
    {
        builder.AddKafkaConsumer<string, MessageEnvelop>(connectionName, configureSettings: (settings) => {
            settings.Config.GroupId = groupId;
            settings.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
        },
        configureBuilder: (builder) =>
        {
            builder.SetValueDeserializer(new MessageEnvelopDeserializer());
        }
        );

        return builder;
    }
}

internal class MessageEnvelopDeserializer : IDeserializer<MessageEnvelop>
{
    public MessageEnvelop Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        return JsonSerializer.Deserialize<MessageEnvelop>(data) ?? throw new Exception("Error deserialize data");
    }
}

internal class MessageEnvelopSerializer : ISerializer<MessageEnvelop>
{
    public byte[] Serialize(MessageEnvelop data, SerializationContext context)
    {
        return JsonSerializer.SerializeToUtf8Bytes(data);
    }
}