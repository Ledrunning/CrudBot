namespace CrudBot.Exceptions.Exceptions;

public class CrudBotException : Exception
{
    private readonly string? _errorCode;
    private readonly string? _errorMessage;

    public CrudBotException(string message) : base(message)
    {
    }

    public CrudBotException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public CrudBotException(string message, string errorCode, string errorMessage) : base(message)
    {
        _errorCode = errorCode;
        _errorMessage = errorMessage;
    }
}