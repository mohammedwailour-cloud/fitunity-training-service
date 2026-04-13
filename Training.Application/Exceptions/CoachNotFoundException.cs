namespace Training.Application.Exceptions;

public class CoachNotFoundException : Exception
{
    public CoachNotFoundException(Guid id)
        : base($"Coach {id} not found")
    {
    }
}
