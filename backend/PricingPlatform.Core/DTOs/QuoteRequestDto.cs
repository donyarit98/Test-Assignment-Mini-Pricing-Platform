namespace PricingPlatform.Core.DTOs;

public class QuoteRequestDto
{
    public decimal WeightKg { get; set; }
    public string DestinationZipCode { get; set; } = string.Empty;
    public DateTime ShipmentDate { get; set; }
    public decimal DeclaredValue { get; set; }
}
