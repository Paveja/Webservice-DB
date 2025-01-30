using Microsoft.EntityFrameworkCore;
using dotnet06.Data;

var builder = WebApplication.CreateBuilder(args);

// Explicitly load environment variables
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddEnvironmentVariables();

// Add essential services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

// Retrieve individual connection variables
var dbHost = builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException("Environment variable 'DATABASE_URL' is not set.");
var dbPort = builder.Configuration["DATABASE_PORT"] ?? "3306"; // Default to 3306 if not provided
var dbName = builder.Configuration["DATABASE_NAME"]
    ?? throw new InvalidOperationException("Environment variable 'DATABASE_NAME' is not set.");
var dbUser = builder.Configuration["DATABASE_USERNAME"]
    ?? throw new InvalidOperationException("Environment variable 'DATABASE_USER' is not set.");
var dbPassword = builder.Configuration["DATABASE_PASSWORD"]
    ?? throw new InvalidOperationException("Environment variable 'DATABASE_PASSWORD' is not set.");

var baseUrl = builder.Configuration["BASE_URL"] ?? "/api/";

// Build the connection string
var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};User={dbUser};Password={dbPassword};AllowPublicKeyRetrieval=True";

Console.WriteLine($"Built Connection String: {connectionString}");

// Configure the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

var app = builder.Build();

// Apply database migrations
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Migration error: {ex.Message}");
}

// Enable Swagger in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UsePathBase(baseUrl);
app.UseRouting();
// Map controllers
app.MapControllers();
app.Run();
