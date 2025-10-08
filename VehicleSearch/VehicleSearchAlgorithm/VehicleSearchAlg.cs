using System.ComponentModel.Design;
using Objects;
using System.Linq;
using System.Text.Json;

namespace VehicleSearchAlgorithm;
//The results should:
//
// Include every possible location that could store all requested vehicles
// Include the cheapest possible combination of listings per location
//     Include only one result per location_id
//     Be sorted by the total price in cents, ascending
//     Assumptions
// To simplify the problem, you should make two other assumptions:
//
// Assume that, in each listing, vehicles will be stored at the same orientation
//     Assume that no buffer space is needed between vehicles


public static class VehicleSearchAlg
{
    public static void Main(string[] args)
    {

        // Load test JSON
        string listingsJson =
            File.ReadAllText(
                @"C:\Users\watri\RiderProjects\vehicle-search\VehicleSearch\VehicleSearchAlgorithm\listings.json");
        List<Listing> listings = JsonSerializer.Deserialize<List<Listing>>(listingsJson);

        string vehiclesJson =
            File.ReadAllText(
                @"C:\Users\watri\RiderProjects\vehicle-search\VehicleSearch\VehicleSearchAlgorithm\vehicles.json");
        List<Vehicle> vehicles = JsonSerializer.Deserialize<List<Vehicle>>(vehiclesJson);

        // Call your algorithm
        var results = VehicleSearchAlg.FindLocations(vehicles, listings);

        // Print results
        foreach (var r in results)
        {
            Console.WriteLine(
                $"Location: {r.location_id}, Total Price: {r.total_price_in_cents}, Listings: {string.Join(",", r.listing_ids)}");
        }
    }

    public static List<SearchResult> FindLocations(List<Vehicle> vehicles, List<Listing> listings)
    {


        List<SearchResult> results = new List<SearchResult>();
        //Group listings by their location_id
        Dictionary<string, List<Listing>> groupedByLocation =
            listings.GroupBy(l => l.location_id).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var listingPair in groupedByLocation)
        {
            string locationId = listingPair.Key;
            List<Listing> locations = listingPair.Value;
            var bestFit = FitVehicles(vehicles, locations);
            if (bestFit != null)
            {
                results.Add(bestFit);
            }


        }

        return results.OrderBy(r => r.total_price_in_cents).ToList();
    }

private static SearchResult? FitVehicles(List<Vehicle> vehicles, List<Listing> listings)
{
    // Flatten vehicles into individual units
    List<Vehicle> vehicleUnits = new List<Vehicle>();
    foreach (var v in vehicles)
        for (int i = 0; i < v.quantity; i++)
            vehicleUnits.Add(v);

    SearchResult? bestResult = null;
    int bestCost = int.MaxValue;

    // Generate all subsets of listings (powerset)
    int n = listings.Count;
    for (int mask = 1; mask < (1 << n); mask++) // skip mask=0 (no listings)
    {
        List<Listing> combo = new List<Listing>();
        int totalCost = 0;
        foreach (int i in Enumerable.Range(0, n))
        {
            if ((mask & (1 << i)) != 0)
            {
                combo.Add(listings[i]);
                totalCost += listings[i].price_in_cents;
            }
        }

        // Check if this combination can fit all vehicles
        if (CanFitAllVehicles(vehicleUnits, combo))
        {
            if (totalCost < bestCost)
            {
                bestCost = totalCost;
                bestResult = new SearchResult
                {
                    location_id = combo[0].location_id,
                    listing_ids = combo.Select(l => l.id).ToList(),
                    total_price_in_cents = totalCost
                };
            }
        }
    }

    return bestResult;
}

// Simple area-based check: try widthwise and lengthwise orientations
private static bool CanFitAllVehicles(List<Vehicle> vehicleUnits, List<Listing> listings)
{
    // Calculate total vehicle area
    int totalVehicleArea = vehicleUnits.Sum(v => v.length * 10); // width always 10
    int totalListingAreaWidthwise = listings.Sum(l => l.width * l.length);
    int totalListingAreaLengthwise = listings.Sum(l => l.width * l.length); // same for now

    // Quick approximation: if total area fits, assume it's packable
    return totalVehicleArea <= totalListingAreaWidthwise || totalVehicleArea <= totalListingAreaLengthwise;
}



}



