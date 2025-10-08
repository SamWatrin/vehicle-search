using System.Text.Json;
using Objects; // namespace from your objects project
using VehicleSearchAlgorithm; // namespace where VehicleSearchAlg lives

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Load listings at startup (from data/listings.json)
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

// POST endpoint: receives vehicles, uses in-project listings
app.MapPost("/search", (List<Vehicle> vehicles) =>
{
    // Call your search algorithm
    // Adjust this call to match your actual method signature
    var results = VehicleSearchAlg.FindLocations(vehicles, listings);

    return Results.Ok(results);
});

app.Run();