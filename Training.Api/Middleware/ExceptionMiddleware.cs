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

        switch (exception)
        {
            case ReservationNotFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                break;

            case InvalidReservationStateException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case SessionFullException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case DuplicateReservationException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                break;

            case Exception:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                break;
        }

        var result = new
        {
            error = exception.Message
        };

        return response.WriteAsJsonAsync(result);
    }
}