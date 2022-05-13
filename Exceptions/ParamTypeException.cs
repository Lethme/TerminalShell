namespace Terminal.Exceptions;

public class ParamTypeException : Exception
{
    internal ParamTypeException()
    {
        
    }
    internal ParamTypeException(string message)
        : base(message)
    {
        
    }
    internal ParamTypeException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}