namespace Training.Application.Sessions.DTOs
{
    public class UpdateSessionRequest
    {
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public int? Capacite { get; set; }
        public decimal? Prix { get; set; }
        public bool AbonnementRequis { get; set; }
        public Guid SpaceId { get; set; }
        public Guid? CoachId { get; set; }
    }
}
