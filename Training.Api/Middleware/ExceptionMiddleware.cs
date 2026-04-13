using Training.Application.Exceptions;
using Training.Domain.Exceptions;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = exception switch
        {
            ReservationNotFoundException => StatusCodes.Status404NotFound,
            SessionNotFoundException => StatusCodes.Status404NotFound,
            CoachNotFoundException => StatusCodes.Status404NotFound,
            InvalidReservationStateException => StatusCodes.Status400BadRequest,
            InvalidReservationUserException => StatusCodes.Status400BadRequest,
            InvalidReservationSessionException => StatusCodes.Status400BadRequest,
            InvalidSessionDatesException => StatusCodes.Status400BadRequest,
            InvalidSessionCapacityException => StatusCodes.Status400BadRequest,
            InvalidSessionPriceException => StatusCodes.Status400BadRequest,
            InvalidSessionStateException => StatusCodes.Status400BadRequest,
            InvalidActivityNameException => StatusCodes.Status400BadRequest,
            InvalidCoachNameException => StatusCodes.Status400BadRequest,
            InvalidCoachEmailException => StatusCodes.Status400BadRequest,
            InvalidCoachActivityException => StatusCodes.Status400BadRequest,
            InvalidEventTitleException => StatusCodes.Status400BadRequest,
            InvalidEventDateException => StatusCodes.Status400BadRequest,
            InvalidEventCapacityException => StatusCodes.Status400BadRequest,
            CoachActivityMismatchException => StatusCodes.Status400BadRequest,
            SessionFullException => StatusCodes.Status409Conflict,
            DuplicateReservationException => StatusCodes.Status409Conflict,
            SessionCapacityConflictException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var result = new
        {
            statusCode = response.StatusCode,
            error = GetErrorCode(exception),
            message = GetErrorMessage(exception)
        };

        return response.WriteAsJsonAsync(result);
    }

    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            ReservationNotFoundException => "reservation_not_found",
            SessionNotFoundException => "session_not_found",
            CoachNotFoundException => "coach_not_found",
            InvalidReservationStateException => "invalid_reservation_state",
            InvalidReservationUserException => "invalid_reservation_user",
            InvalidReservationSessionException => "invalid_reservation_session",
            InvalidSessionDatesException => "invalid_session_dates",
            InvalidSessionCapacityException => "invalid_session_capacity",
            InvalidSessionPriceException => "invalid_session_price",
            InvalidSessionStateException => "invalid_session_state",
            InvalidActivityNameException => "invalid_activity_name",
            InvalidCoachNameException => "invalid_coach_name",
            InvalidCoachEmailException => "invalid_coach_email",
            InvalidCoachActivityException => "invalid_coach_activity",
            InvalidEventTitleException => "invalid_event_title",
            InvalidEventDateException => "invalid_event_date",
            InvalidEventCapacityException => "invalid_event_capacity",
            CoachActivityMismatchException => "coach_activity_mismatch",
            SessionFullException => "session_full",
            DuplicateReservationException => "duplicate_reservation",
            SessionCapacityConflictException => "session_capacity_conflict",
            ArgumentException => "invalid_request",
            _ => "internal_server_error"
        };
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            ReservationNotFoundException => "Reservation introuvable",
            SessionNotFoundException => "Session introuvable",
            CoachNotFoundException => "Coach introuvable",
            InvalidReservationStateException => "Etat de reservation invalide",
            InvalidReservationUserException => "Utilisateur de reservation invalide",
            InvalidReservationSessionException => "Session de reservation invalide",
            InvalidSessionDatesException => "Date de session invalide",
            InvalidSessionCapacityException => "Capacite invalide",
            InvalidSessionPriceException => "Prix invalide",
            InvalidSessionStateException => "Etat de session invalide",
            InvalidActivityNameException => "Nom d'activite invalide",
            InvalidCoachNameException => "Nom de coach invalide",
            InvalidCoachEmailException => "Email de coach invalide",
            InvalidCoachActivityException => "Activite du coach invalide",
            InvalidEventTitleException => "Titre d'evenement invalide",
            InvalidEventDateException => "Date d'evenement invalide",
            InvalidEventCapacityException => "Capacite d'evenement invalide",
            CoachActivityMismatchException => "Le coach n'appartient pas a l'activite de la session",
            SessionFullException => "Session complete",
            DuplicateReservationException => "Reservation deja existante pour cet utilisateur",
            SessionCapacityConflictException => "La capacite ne peut pas etre inferieure au nombre de reservations existantes",
            ArgumentException => "Requete invalide",
            _ => "Une erreur interne est survenue"
        };
    }
}
