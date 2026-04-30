namespace TaskManager.Domain.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string name, object key)
        : base($"{name} with key {key} was not found.", 404)
    {
    }
}
