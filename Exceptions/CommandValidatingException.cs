namespace Terminal.Exceptions;

public class CommandValidatingException : Exception
{
    internal CommandValidatingException()
    {
        
    }
    internal CommandValidatingException(string message)
        : base(message)
    {
        
    }
    internal CommandValidatingException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}