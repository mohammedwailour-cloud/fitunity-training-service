namespace Training.Application.Exceptions;

public class SpaceNotFoundException : Exception
{
    public SpaceNotFoundException(Guid id)
        : base($"Space {id} not found")
    {
    }
}
