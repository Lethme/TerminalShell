namespace Terminal.Exceptions;

public class CommandNotFoundException : Exception
{
    internal CommandNotFoundException()
    {
        
    }
    internal CommandNotFoundException(string message)
        : base(message)
    {
        
    }
    internal CommandNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}