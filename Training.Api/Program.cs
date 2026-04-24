using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Training.Api.Security;
using Training.Application.Activities.Interfaces;
using Training.Application.Activities.UseCases;
using Training.Application.Activities.UseCases.Training.Application.Activities.UseCases;
using Training.Application.Calendar.Interfaces;
using Training.Application.Calendar.UseCases;
using Training.Application.Coachs.Interfaces;
using Training.Application.Coachs.UseCases;
using Training.Application.Common.Interfaces;
using Training.Application.Common.Security;
using Training.Application.Events.Interfaces;
using Training.Application.Events.UseCases;
using Training.Application.Reservations.Interfaces;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.UseCases;
using Training.Application.Spaces.Interfaces;
using Training.Application.Spaces.UseCases;
using Training.Infrastructure.Events;
using Training.Infrastructure.Persistence;
using Training.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
JwtOptions jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT Bearer token. Example: Bearer <token>"
        };

        return Task.CompletedTask;
    });

    options.AddOperationTransformer((operation, context, _) =>
    {
        IReadOnlyList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata.ToList().AsReadOnly();
        bool isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        bool requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

        if (isAnonymous || !requiresAuthorization)
        {
            return Task.CompletedTask;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, JwtUserContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            NameClaimType = JwtClaimNames.UserId,
            RoleClaimType = JwtClaimNames.Role,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddDbContext<TrainingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<GetSessionUseCase>();
builder.Services.AddScoped<CreateSessionUseCase>();
builder.Services.AddScoped<GetAllSessionsUseCase>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<ReserveSessionUseCase>();

builder.Services.AddScoped<ConfirmReservationUseCase>();
builder.Services.AddScoped<CancelReservationUseCase>();
builder.Services.AddScoped<GetReservationsByUserUseCase>();
builder.Services.AddScoped<GetSessionsPagedUseCase>();
builder.Services.AddScoped<GetReservationsPagedUseCase>();
builder.Services.AddScoped<GetReservationsBySessionUseCase>();
builder.Services.AddScoped<ICalendarRepository, CalendarRepository>();
builder.Services.AddScoped<GetUserCalendarUseCase>();

builder.Services.AddScoped<IEventPublisher, EventPublisher>();

builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<CreateActivityUseCase>();
builder.Services.AddScoped<GetActivityByIdUseCase>();
builder.Services.AddScoped<GetActivitiesUseCase>();
builder.Services.AddScoped<UpdateActivityUseCase>();
builder.Services.AddScoped<DeleteActivityUseCase>();
builder.Services.AddScoped<DeleteSessionUseCase>();
builder.Services.AddScoped<UpdateSessionUseCase>();

builder.Services.AddScoped<ICoachRepository, CoachRepository>();
builder.Services.AddScoped<CreateCoachUseCase>();
builder.Services.AddScoped<GetCoachByIdUseCase>();
builder.Services.AddScoped<GetCoachesUseCase>();
builder.Services.AddScoped<DeleteCoachUseCase>();
builder.Services.AddScoped<UpdateCoachUseCase>();

builder.Services.AddScoped<IEventRepository, EventRepository>();

builder.Services.AddScoped<CreateEventUseCase>();
builder.Services.AddScoped<GetEventByIdUseCase>();
builder.Services.AddScoped<GetEventsUseCase>();
builder.Services.AddScoped<UpdateEventUseCase>();
builder.Services.AddScoped<DeleteEventUseCase>();

builder.Services.AddScoped<ISpaceRepository, SpaceRepository>();
builder.Services.AddScoped<CreateSpaceUseCase>();
builder.Services.AddScoped<UpdateSpaceUseCase>();
builder.Services.AddScoped<GetSpaceByIdUseCase>();
builder.Services.AddScoped<GetSpacesUseCase>();
builder.Services.AddScoped<DeleteSpaceUseCase>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddPreferredSecuritySchemes(["Bearer"]);
    });

    app.MapGet("/debug/auth/claims", (HttpContext httpContext, IUserContext userContext) =>
    {
        IEnumerable<object> claims = httpContext.User.Claims
            .Select(claim => new
            {
                claim.Type,
                claim.Value
            });

        return Results.Ok(new
        {
            isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false,
            authenticationType = httpContext.User.Identity?.AuthenticationType,
            userId = userContext.UserId,
            role = userContext.Role,
            hasActiveSubscription = userContext.HasActiveSubscription,
            claims
        });
    })
    .RequireAuthorization()
    .WithName("GetCurrentUserClaims");
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
