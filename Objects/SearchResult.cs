namespace Objects;

public class SearchResult
{
    public string location_id { get; set; }
    public List<string> listing_ids { get; set; } = new List<string>();
    public int total_price_in_cents { get; set; }

}