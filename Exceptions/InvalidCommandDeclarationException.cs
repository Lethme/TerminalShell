namespace Terminal.Exceptions;

public class InvalidCommandDeclarationException : Exception
{
    internal InvalidCommandDeclarationException()
    {
        
    }
    internal InvalidCommandDeclarationException(string message)
        : base(message)
    {
        
    }
    internal InvalidCommandDeclarationException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}