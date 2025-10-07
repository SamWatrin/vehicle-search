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
        Dictionary<Listing, Dictionary<string, Dictionary<Vehicle, int>>> capacities =
            new Dictionary<Listing, Dictionary<string, Dictionary<Vehicle, int>>>();
        
        foreach (Listing location in listings)
        {
            Dictionary<string, Dictionary<Vehicle, int>> orientationCap = new Dictionary<string, Dictionary<Vehicle, int>>();
            orientationCap["widthwise"] = new Dictionary<Vehicle, int>();
            orientationCap["lengthwise"] = new Dictionary<Vehicle, int>();
            foreach (Vehicle vehicle in vehicles)
            {
                //widthwise
                int widthW= location.width / 10;
                int lengthW = location.length / vehicle.length;
                orientationCap["widthwise"][vehicle] = Math.Min(widthW, lengthW);
                //lengthwise
                int widthL = location.length / 10;
                int lengthL = location.width / vehicle.length;
                orientationCap["lengthwise"][vehicle] = Math.Min(widthL, lengthL);

            }
           
            capacities[location] = orientationCap;
        }
      

        //
        SearchResult? widthResult = FindCheapestAssignmentForOrientation(vehicles, listings,capacities,"widthwise");
        SearchResult? lengthResult =  FindCheapestAssignmentForOrientation(vehicles, listings,capacities,  "lengthwise");
        if (widthResult == null && lengthResult != null)
        {
            return lengthResult;
        }
        else if (lengthResult == null)
        {
            return widthResult;
        }
        else
        {
            if (widthResult.total_price_in_cents <= lengthResult.total_price_in_cents)
            {
                return widthResult;
            }
            else
            {
                return lengthResult;
            }
        }
    }
    
    
public static SearchResult? FindCheapestAssignmentForOrientation(
    List<Vehicle> vehicles,
    List<Listing> listings,
    Dictionary<Listing, Dictionary<string, Dictionary<Vehicle, int>>> capacities,
    string orientation)
{
    if (vehicles.Count == 0 || listings.Count == 0) return null;

    // Step 1: Flatten vehicles based on quantity
    List<Vehicle> vehicleUnits = new List<Vehicle>();
    foreach (Vehicle v in vehicles)
    {
        for (int i = 0; i < v.quantity; i++)
            vehicleUnits.Add(v);
    }

    SearchResult? bestResult = null;
    int bestCost = int.MaxValue;

    // Step 2: DFS 
  void DFS(
    int vehicleIndex,
    Dictionary<Listing, Dictionary<Vehicle,int>> usedCapacity,
    int currentCost,
    List<Listing> listingsUsed)
{
    // Base case: all vehicles placed
    if (vehicleIndex == vehicleUnits.Count)
    {
        if (currentCost < bestCost)
        {
            bestCost = currentCost;
            bestResult = new SearchResult
            {
                location_id = listings[0].location_id,
                listing_ids = listingsUsed.Select(l => l.id).ToList(),
                total_price_in_cents = currentCost
            };
        }
        return;
    }

    var vehicle = vehicleUnits[vehicleIndex];

    // Try to place this vehicle in each listing
    foreach (Listing listing in listings)
    {
        int used = usedCapacity.ContainsKey(listing) && usedCapacity[listing].ContainsKey(vehicle)
            ? usedCapacity[listing][vehicle]
            : 0;

        int available = capacities[listing][orientation][vehicle] - used;

        if (available <= 0) continue;

        // Determine added cost
        int addedCost = listingsUsed.Contains(listing) ? 0 : listing.price_in_cents;

        // Prune if this path is already more expensive than best
        if (currentCost + addedCost >= bestCost) continue;

        // --- Update state ---
        if (!usedCapacity.ContainsKey(listing))
            usedCapacity[listing] = new Dictionary<Vehicle,int>();

        if (!usedCapacity[listing].ContainsKey(vehicle))
            usedCapacity[listing][vehicle] = 0;

        usedCapacity[listing][vehicle]++;

        bool addedListing = false;
        if (!listingsUsed.Contains(listing))
        {
            listingsUsed.Add(listing);
            addedListing = true;
        }

        // --- Recurse ---
        DFS(vehicleIndex + 1, usedCapacity, currentCost + addedCost, listingsUsed);

        // --- Backtrack ---
        usedCapacity[listing][vehicle]--;
        if (usedCapacity[listing][vehicle] == 0)
            usedCapacity[listing].Remove(vehicle);
        if (usedCapacity[listing].Count == 0)
            usedCapacity.Remove(listing);

        if (addedListing)
            listingsUsed.Remove(listing);
    }
}

    DFS(0, new Dictionary<Listing, Dictionary<Vehicle,int>>(), 0, new List<Listing>());

    return bestResult;
}






}



