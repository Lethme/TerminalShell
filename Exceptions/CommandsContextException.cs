namespace Terminal.Exceptions;

public class CommandsContextException : Exception
{
    internal CommandsContextException()
    {
        
    }
    internal CommandsContextException(string message)
        : base(message)
    {
        
    }
    internal CommandsContextException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}