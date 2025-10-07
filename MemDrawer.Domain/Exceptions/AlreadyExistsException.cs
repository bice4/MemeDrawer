namespace MemDrawer.Domain.Exceptions;

public class AlreadyExistsException : DomainException
{
    public AlreadyExistsException(string errorMessage)
    {
        ErrorMessage = errorMessage;
        HttpStatusCode = 409;
    }

    private string ErrorMessage { get; }
    public override string ToResponseMessage() => ErrorMessage;
}