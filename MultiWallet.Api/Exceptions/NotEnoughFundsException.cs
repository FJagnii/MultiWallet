namespace MultiWallet.Api.Exceptions;

public class NotEnoughFundsException : Exception
{
    public NotEnoughFundsException(decimal funds) : base($"Niewystarczające środki do wykonania operacji ({funds})") { }
    
    public NotEnoughFundsException(string message, Exception innerException) : base(message, innerException) { }
}