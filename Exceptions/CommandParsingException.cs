namespace Terminal.Exceptions;

public class CommandParsingException : Exception
{
    internal CommandParsingException()
    {
        
    }
    internal CommandParsingException(string message)
        : base(message)
    {
        
    }
    internal CommandParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}