namespace Terminal.Exceptions;

public class UnsupportedParamTypeException : Exception
{
    internal UnsupportedParamTypeException()
    {
        
    }
    internal UnsupportedParamTypeException(string message)
        : base(message)
    {
        
    }
    internal UnsupportedParamTypeException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}