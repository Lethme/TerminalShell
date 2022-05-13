namespace Terminal.Exceptions;

public class AliasesDuplicationException : Exception
{
    internal AliasesDuplicationException()
    {
        
    }
    internal AliasesDuplicationException(string message)
        : base(message)
    {
        
    }
    internal AliasesDuplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}