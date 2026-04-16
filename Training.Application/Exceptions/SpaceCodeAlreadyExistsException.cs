namespace Training.Application.Exceptions;

public class SpaceCodeAlreadyExistsException : Exception
{
    public SpaceCodeAlreadyExistsException(string code)
        : base($"Space code '{code}' already exists")
    {
    }
}
