namespace Training.Domain.Exceptions;

public class CoachActivityMismatchException : Exception
{
    public CoachActivityMismatchException(Guid coachId, Guid activityId)
        : base($"Coach {coachId} does not belong to activity {activityId}")
    {
    }
}
