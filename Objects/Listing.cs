namespace Objects;

public class Listing
{
    public string id { get; set; }
    public string location_id { get; set; }
    public int length { get; set; }
    public int width { get; set; }
    public int price_in_cents { get; set; }
    
    public override bool Equals(object obj)
    {
        return obj is Listing other && id == other.id;
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

}