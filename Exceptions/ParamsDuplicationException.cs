namespace Terminal.Exceptions;

public class ParamsDuplicationException : Exception
{
    internal ParamsDuplicationException()
    {
        
    }
    internal ParamsDuplicationException(string message)
        : base(message)
    {
        
    }
    internal ParamsDuplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}