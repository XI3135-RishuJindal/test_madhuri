namespace UserManagementService.Exceptions;

/// <summary>
/// Exception thrown when a conflict occurs (e.g., duplicate resource).
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
