using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Training.Application.Activities.Interfaces;
using Training.Application.Activities.UseCases;
using Training.Application.Activities.UseCases.Training.Application.Activities.UseCases;
using Training.Application.Common.Interfaces;
using Training.Application.Reservations.Interfaces;
using Training.Application.Sessions.Interfaces;
using Training.Application.Sessions.UseCases;
using Training.Infrastructure.Events;
using Training.Infrastructure.Persistence;
using Training.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();
