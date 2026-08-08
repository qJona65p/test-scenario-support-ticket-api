using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SupportTicketApi.Data;
using SupportTicketApi.Middleware;
using SupportTicketApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
                      ?? "Data Source=support-tickets.db"));

// --- application services (registered as features are added) ---
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<ITicketService, TicketService>();
// --- end application services ---

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Applying migrations at startup keeps the trainee experience to a single
// "dotnet run" with no database setup step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

app.Run();

/// <summary>Exposed so integration tests can bootstrap the real pipeline.</summary>
public partial class Program
{
}
