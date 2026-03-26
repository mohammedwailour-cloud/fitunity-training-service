namespace Training.Application.Exceptions;

public class InvalidReservationStateException : Exception
{
    public InvalidReservationStateException(string message)
        : base(message)
    {
    }
}
