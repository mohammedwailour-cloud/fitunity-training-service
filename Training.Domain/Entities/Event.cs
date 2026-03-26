using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Training.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Titre { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public int Capacite { get; private set; }

    private readonly List<Session> _sessions = new();
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

    private Event() { }

    public Event(string titre, string description, DateTime date, int capacite)
    {
        if (capacite <= 0)
            throw new Exception("La capacité doit être positive.");

        Id = Guid.NewGuid();
        Titre = titre;
        Description = description;
        Date = date;
        Capacite = capacite;
    }

    public void AjouterSession(Session session)
    {
        _sessions.Add(session);
    }
}