namespace Terminal.Exceptions;

public class UnsupportedParamCollectionTypeException : Exception
{
    internal UnsupportedParamCollectionTypeException()
    {
        
    }
    internal UnsupportedParamCollectionTypeException(string message)
        : base(message)
    {
        
    }
    internal UnsupportedParamCollectionTypeException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}