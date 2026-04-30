namespace TaskManager.Domain.Exceptions;

public class ValidationException : AppException
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(IEnumerable<string> errors)
        : base("Validation failed.", 400)
    {
        Errors = errors;
    }

    public ValidationException(string message)
        : base(message, 400)
    {
        Errors = new[] { message };
    }
}
