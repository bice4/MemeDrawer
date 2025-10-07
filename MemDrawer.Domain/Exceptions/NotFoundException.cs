namespace MemDrawer.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public int HttpStatusCode { get; protected init; } = 500;

    public abstract string ToResponseMessage();
}

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, string? entityId)
    {
        EntityName = entityName;
        EntityId = entityId;
        HttpStatusCode = 404;
    }

    private string EntityName { get; }
    private string? EntityId { get; }

    public override string ToResponseMessage() => $"{EntityName} with id: {EntityId} not found";
}