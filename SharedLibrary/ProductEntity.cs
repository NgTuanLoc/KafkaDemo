namespace SharedLibrary;

public class ProductEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    private readonly List<IDomainEvent> domainEvents = [];

    public void RegisterDomainEvent(IDomainEvent domainEvent)
    {
        domainEvents.Add(domainEvent);
    }

    public IEnumerable<IDomainEvent> GetEvents()
    {
        return [.. domainEvents];
    }

    public void ClearEvents()
    {
        domainEvents.Clear();
    }
}

public interface IDomainEvent
{
    public string? PartitionKey { get; }
}