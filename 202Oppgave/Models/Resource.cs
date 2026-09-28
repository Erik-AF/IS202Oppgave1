using System;

namespace _202Oppgave.Models;

public class Resource
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Area { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string Status { get; set; } = "Tilgjengelig";
    public string? OwnerName { get; set; }
}