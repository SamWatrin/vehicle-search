
using System.Text.Json;
using Objects; // domain classes
using VehicleSearchAlgorithm; // your algorithm namespace

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Load listings once at startup from data/listings.json
var listingsPath = Path.Combine(app.Environment.ContentRootPath, "data", "listings.json");
List<Listing> listings = new();
if (File.Exists(listingsPath))
{
    var json = File.ReadAllText(listingsPath);
    listings = JsonSerializer.Deserialize<List<Listing>>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }) ?? new List<Listing>();
}

// POST endpoint: takes vehicles from request body, uses listings in memory
app.MapPost("/search", (List<Vehicle> vehicles) =>
{
    var results = VehicleSearchAlg.FindLocations(vehicles, listings);
    return Results.Ok(results);
});

app.Run();
