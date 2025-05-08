namespace Tutorial8.Models.DTOs;

public class ClientTripDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public int MaxPeople { get; set; }

    public int? PaymentDate { get; set; } // ?
    public int? RegisteredAt { get; set; } // ?
}
