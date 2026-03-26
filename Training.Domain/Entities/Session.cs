using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Training.Domain.Enums;



namespace Training.Domain.Entities;

public class Session
{
    public Guid Id { get; set; } // xx ici j'ai rendu set public pour que je puisse l'affecter un guid depuis le usecase 

    public SessionType Type { get; private set; }

    public DateTime DateDebut { get; private set; }
    public DateTime DateFin { get; private set; }

    public int? Capacite { get; private set; }
    public decimal? Prix { get; private set; }

    public bool AbonnementRequis { get; private set; }

    public Guid? ActivityId { get; private set; }
    public Guid? CoachId { get; private set; }
    public Guid? EventId { get; private set; }

    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private Session() { }

    public Session(
        SessionType type,
        DateTime dateDebut,
        DateTime dateFin,
        int? capacite,
        decimal? prix,
        bool abonnementRequis,
        Guid? activityId = null,
        Guid? coachId = null,
        Guid? eventId = null)
    {
        if (dateFin <= dateDebut)
            throw new Exception("Date invalide.");

        if (capacite.HasValue && capacite <= 0)
            throw new Exception("Capacité invalide.");

        Id = Guid.NewGuid();
        Type = type;
        DateDebut = dateDebut;
        DateFin = dateFin;
        Capacite = capacite;
        Prix = prix;
        AbonnementRequis = abonnementRequis;
        ActivityId = activityId;
        CoachId = coachId;
        EventId = eventId;
    }

    public void AjouterReservation(Guid userId)
    {
        if (Capacite.HasValue && _reservations.Count >= Capacite)
            throw new Exception("Session complète.");

        var reservation = new Reservation(userId, Id);
        _reservations.Add(reservation);
    }
    public bool IsFull(int currentReservations)
    {
        if (!Capacite.HasValue)
            return false;

        return currentReservations >= Capacite.Value;
    }

    public bool IsInPast()
    {
        return DateDebut < DateTime.UtcNow;
    }
}