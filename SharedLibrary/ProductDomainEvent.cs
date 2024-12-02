namespace SharedLibrary;

public record ProductDomainEvent(Guid Id, string Name, string Description) : IDomainEvent
{
    public string? PartitionKey => Id.ToString();
}

public sealed record ProductCreated(Guid Id, string Name, string Description) : ProductDomainEvent(Id, Name, Description);