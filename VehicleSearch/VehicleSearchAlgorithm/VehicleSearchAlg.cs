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
        string listingsJson = File.ReadAllText(@"C:\Users\watri\RiderProjects\vehicle-search\VehicleSearch\VehicleSearchAlgorithm\listings.json");
        List<Listing> listings = JsonSerializer.Deserialize<List<Listing>>(listingsJson);

        string vehiclesJson = File.ReadAllText(@"C:\Users\watri\RiderProjects\vehicle-search\VehicleSearch\VehicleSearchAlgorithm\vehicles.json");
        List<Vehicle> vehicles = JsonSerializer.Deserialize<List<Vehicle>>(vehiclesJson);

        // Call your algorithm
        var results = VehicleSearchAlg.FindLocations(vehicles, listings);

        // Print results
        foreach (var r in results)
        {
            Console.WriteLine($"Location: {r.location_id}, Total Price: {r.total_price_in_cents}, Listings: {string.Join(",", r.listing_ids)}");
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
        // capacities: Listing -> orientation -> vehicleKey -> capacity
        Dictionary<Listing, Dictionary<string, Dictionary<string, int>>> capacities =
            new Dictionary<Listing, Dictionary<string, Dictionary<string, int>>>();

        foreach (Listing listing in listings)
        {
            Dictionary<string, Dictionary<string, int>> orientationCap = new Dictionary<string, Dictionary<string, int>>();
            orientationCap["widthwise"] = new Dictionary<string, int>();
            orientationCap["lengthwise"] = new Dictionary<string, int>();

            foreach (Vehicle vehicle in vehicles)
            {
                string vehicleKey = vehicle.length.ToString(); // simple unique key

                // widthwise
                int widthwiseCapacity = (listing.width / 10) * (listing.length / vehicles[0].length);
                orientationCap["widthwise"][vehicleKey] = widthwiseCapacity;

                // lengthwise
                int lengthwiseCapacity = (listing.length / 10) * (listing.width / vehicles[0].length);
                orientationCap["lengthwise"][vehicleKey] = lengthwiseCapacity;
            }

            capacities[listing] = orientationCap;
        }

        // Run DFS for both orientations
        var widthResult = FindCheapestAssignmentForOrientation(vehicles, listings, capacities, "widthwise");
        var lengthResult = FindCheapestAssignmentForOrientation(vehicles, listings, capacities, "lengthwise");

        if (widthResult == null) return lengthResult;
        if (lengthResult == null) return widthResult;

        return widthResult.total_price_in_cents <= lengthResult.total_price_in_cents
            ? widthResult
            : lengthResult;
    }

    
    
public static SearchResult? FindCheapestAssignmentForOrientation(
    List<Vehicle> vehicles,
    List<Listing> listings,
    Dictionary<Listing, Dictionary<string, Dictionary<string, int>>> capacities,
    string orientation)
{
    if (vehicles.Count == 0 || listings.Count == 0) return null;

    // Flatten vehicles based on quantity
    List<Vehicle> vehicleUnits = new List<Vehicle>();
    foreach (Vehicle v in vehicles)
        for (int i = 0; i < v.quantity; i++)
            vehicleUnits.Add(v);

    SearchResult? bestResult = null;
    int bestCost = int.MaxValue;

    void DFS(int vehicleIndex, Dictionary<string, Dictionary<string, int>> usedCapacity, int currentCost, List<string> listingsUsedIds)
    {
        if (vehicleIndex == vehicleUnits.Count)
        {
            if (currentCost < bestCost)
            {
                bestCost = currentCost;
                bestResult = new SearchResult
                {
                    location_id = listings[0].location_id,
                    listing_ids = new List<string>(listingsUsedIds),
                    total_price_in_cents = currentCost
                };
            }
            return;
        }

        var vehicle = vehicleUnits[vehicleIndex];
        string vehicleKey = vehicle.length.ToString();

        foreach (Listing listing in listings)
        {
            if (!usedCapacity.ContainsKey(listing.id))
                usedCapacity[listing.id] = new Dictionary<string, int>();

            int used = usedCapacity[listing.id].ContainsKey(vehicleKey)
                ? usedCapacity[listing.id][vehicleKey]
                : 0;

            int available = capacities[listing][orientation][vehicleKey] - used;
            if (available <= 0) continue;

            bool isNewListing = !listingsUsedIds.Contains(listing.id);
            int addedCost = isNewListing ? listing.price_in_cents : 0;

            // Clone for DFS branch
            var newUsedCapacity = usedCapacity.ToDictionary(kvp => kvp.Key, kvp => new Dictionary<string, int>(kvp.Value));
            newUsedCapacity[listing.id][vehicleKey] = used + 1;

            var newListingsUsedIds = new List<string>(listingsUsedIds);
            if (isNewListing)
                newListingsUsedIds.Add(listing.id);

            DFS(vehicleIndex + 1, newUsedCapacity, currentCost + addedCost, newListingsUsedIds);
        }
    }

    DFS(0, new Dictionary<string, Dictionary<string, int>>(), 0, new List<string>());

    return bestResult;
}


}



