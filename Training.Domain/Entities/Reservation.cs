using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Training.Domain.Enums;

namespace Training.Domain.Entities;

public class Reservation
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid SessionId { get; private set; }

    public DateTime DateReservation { get; private set; }

    public ReservationStatus Status { get; private set; }

    private Reservation() { }

    public Reservation(Guid sessionId, Guid userId)
    {
        Id = Guid.NewGuid();
        SessionId = sessionId;
        UserId = userId;
        DateReservation = DateTime.UtcNow;
        Status = ReservationStatus.EnAttente;
    }

    public void Confirm()
    {
        if (Status == ReservationStatus.Annulee)
            throw new Exception("Cannot confirm a cancelled reservation");

        Status = ReservationStatus.Confirmee;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Confirmee)
            throw new Exception("Cannot cancel a confirmed reservation");

        Status = ReservationStatus.Annulee;
    }
   
}
