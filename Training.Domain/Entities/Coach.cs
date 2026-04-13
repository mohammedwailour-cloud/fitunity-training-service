using Training.Domain.Exceptions;

namespace Training.Domain.Entities;

public class Coach
{
    public Guid Id { get; private set; }
    public string Nom { get; private set; }
    public string Email { get; private set; }

    public Guid ActivityId { get; private set; }
    public ActivitySportive Activity { get; private set; }

    private Coach() { }

    public Coach(string nom, string email, Guid activityId)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new InvalidCoachNameException();

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidCoachEmailException();

        if (activityId == Guid.Empty)
            throw new InvalidCoachActivityException();

        Id = Guid.NewGuid();
        Nom = nom;
        Email = email;
        ActivityId = activityId;
    }

    public void Update(string nom, string email)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new InvalidCoachNameException();

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidCoachEmailException();

        Nom = nom;
        Email = email;
    }
}
