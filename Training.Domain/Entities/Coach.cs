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
            throw new ArgumentException("Nom is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        if (activityId == Guid.Empty)
            throw new ArgumentException("ActivityId is required");

        Id = Guid.NewGuid();
        Nom = nom;
        Email = email;
        ActivityId = activityId;
    }

    public void Update(string nom, string email)
    {
        if (string.IsNullOrWhiteSpace(nom))
            throw new ArgumentException("Nom is required");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");

        Nom = nom;
        Email = email;
    }
}