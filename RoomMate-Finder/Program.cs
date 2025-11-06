using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MediatR;
using FluentValidation;
using RoomMate_Finder.Features.Profiles;
using RoomMate_Finder.Infrastructure.Persistence;
using RoomMate_Finder.Validators;
using DotNetEnv;

Env.Load();

var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
if (File.Exists(envFile))
{
    Env.Load(envFile);
}

var builder = WebApplication.CreateBuilder(args);

var host = Environment.GetEnvironmentVariable("DB_HOST");
var port = Environment.GetEnvironmentVariable("DB_PORT");
var database = Environment.GetEnvironmentVariable("DB_NAME");
var username = Environment.GetEnvironmentVariable("DB_USER");
var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

if (string.IsNullOrWhiteSpace(host) ||
    string.IsNullOrWhiteSpace(port) ||
    string.IsNullOrWhiteSpace(database) ||
    string.IsNullOrWhiteSpace(username) ||
    string.IsNullOrWhiteSpace(password))
{
    Console.Error.WriteLine("Missing one or more DB environment variables (DB_HOST, DB_PORT, DB_NAME, DB_USER, DB_PASSWORD).");
    throw new InvalidOperationException("Database environment variables are not set.");
}

var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
var maskedConnectionString = connectionString.Replace(password, "*****");
Console.WriteLine($"Using connection string: {maskedConnectionString}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateProfileRequest>();
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateProfileValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
        Console.WriteLine("Database EnsureCreated() completed.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Database creation failed: {ex.Message}");
        Console.Error.WriteLine(ex.StackTrace);
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapProfilesEndpoints();
app.Run();