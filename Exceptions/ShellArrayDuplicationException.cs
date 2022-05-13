namespace Terminal.Exceptions;

public class ShellArrayDuplicationException : Exception
{
    internal ShellArrayDuplicationException()
    {
        
    }
    internal ShellArrayDuplicationException(string message)
        : base(message)
    {
        
    }
    internal ShellArrayDuplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}