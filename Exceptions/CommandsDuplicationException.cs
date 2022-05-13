namespace Terminal.Exceptions;

public class CommandsDuplicationException : Exception
{
    internal CommandsDuplicationException()
    {
        
    }
    internal CommandsDuplicationException(string message)
        : base(message)
    {
        
    }
    internal CommandsDuplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}