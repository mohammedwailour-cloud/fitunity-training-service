using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Training.Domain.Entities
{
    public class ActivitySportive
    {
        public Guid Id { get; private set; }
        public string Nom { get; private set; }
        public string? Description { get; private set; }

        private readonly List<Session> _sessions = new();
        public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();

        private readonly List<Coach> _coaches = new();
        public IReadOnlyCollection<Coach> Coaches => _coaches.AsReadOnly();

        private ActivitySportive() { }

        public ActivitySportive(string nom, string? description)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Nom is required");

            Id = Guid.NewGuid();
            Nom = nom;
            Description = description;
        }

        public void Update(string nom, string? description)
        {
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Nom is required");

            Nom = nom;
            Description = description;
        }

        public void AjouterSession(Session session)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));

            _sessions.Add(session);
        }

        public void AjouterCoach(Coach coach)
        {
            if (coach == null)
                throw new ArgumentNullException(nameof(coach));

            _coaches.Add(coach);
        }
    }
}
