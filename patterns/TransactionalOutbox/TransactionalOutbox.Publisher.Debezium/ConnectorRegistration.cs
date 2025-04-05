using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TransactionalOutbox.Publisher.Debezium;

public class ConnectorRegistration
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("config")]
    public required ConnectorConfig Config { get; set; }
}

public class ConnectorConfig
{
    [JsonPropertyName("connector.class")]
    public string ConnectorClass { get; set; } = "io.debezium.connector.postgresql.PostgresConnector";

    [JsonPropertyName("database.hostname")]
    public string DatabaseHostname { get; set; } = default!;

    [JsonPropertyName("database.port")]
    public string DatabasePort { get; set; } = default!;

    [JsonPropertyName("database.user")]
    public string DatabaseUser { get; set; } = default!;

    [JsonPropertyName("database.password")]
    public string DatabasePassword { get; set; } = default!;

    [JsonPropertyName("database.dbname")]
    public string DatabaseName { get; set; } = default!;

    [JsonPropertyName("database.server.name")]
    public string DatabaseServerName { get; set; } = default!;

    [JsonPropertyName("plugin.name")]
    public string PluginName { get; set; } = "pgoutput";

    [JsonPropertyName("slot.name")]
    public string SlotName { get; set; } = "debezium";

    [JsonPropertyName("publication.name")]
    public string PublicationName { get; set; } = "dbz_publication";

    [JsonPropertyName("table.include.list")]
    public string TableIncludeList { get; set; } = "";

    [JsonPropertyName("database.history.kafka.bootstrap.servers")]
    public string KafkaBootstrapServers { get; set; } = default!;

    [JsonPropertyName("database.history.kafka.topic")]
    public string KafkaHistoryTopic { get; set; } = "schema-changes.banking";
}
