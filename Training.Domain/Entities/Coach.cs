using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Entities;

public class Coach
{
    public Guid Id { get; private set; }
    public string Nom { get; private set; }
    public string Specialite { get; private set; }

    private readonly List<Session> _sessions = new();
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

    private Coach() { }

    public Coach(string nom, string specialite)
    {
        Id = Guid.NewGuid();
        Nom = nom ?? throw new ArgumentNullException(nameof(nom));
        Specialite = specialite ?? string.Empty;
    }

    public void AssignerSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _sessions.Add(session);
    }
}
