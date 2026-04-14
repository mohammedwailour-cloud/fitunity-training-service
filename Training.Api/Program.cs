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
using Training.Application.Coachs.Interfaces;
using Training.Application.Coachs.UseCases;
using Training.Application.Common.Interfaces;
using Training.Application.Events.Interfaces;
using Training.Application.Events.UseCases;
using Training.Application.Reservations.Interfaces;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.UseCases;
using Training.Infrastructure.Events;
using Training.Infrastructure.Persistence;
using Training.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

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
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;
        var isAnonymous = metadata.OfType<IAllowAnonymous>().Any();
        var requiresAuthorization = metadata.OfType<IAuthorizeData>().Any();

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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddPreferredSecuritySchemes(["Bearer"]);
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();

public partial class Program
{
}


