namespace Training.Application.Exceptions;

public class EventNotFoundException : Exception
{
    public EventNotFoundException(Guid id)
        : base($"Event {id} not found")
    {
    }
}
