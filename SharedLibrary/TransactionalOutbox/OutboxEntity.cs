namespace SharedLibrary.TransactionalOutbox;

public class OutboxEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EventType { get; set; } = string.Empty;

    public string? PartitionKey { get; set; } = null;

    public string Data { get; set; } = string.Empty;

    public DateTime OccurredOn { get; set; }

    public OutboxEntity() { }

    public OutboxEntity(string type, string payload, string? partitionKey = null)
    {
        OccurredOn = DateTime.UtcNow;
        EventType = type;
        Data = payload;
        PartitionKey = partitionKey;
    }
}

