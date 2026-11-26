using System;

namespace _202Oppgave.Models;

/// Visningsmodell for ressursdata. Lagring håndteres av Models.Entities.Resource.
public class Resource
{

    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Area { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public string? GeometryJson { get; set; }

    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }

    public string Status { get; set; } = "Tilgjengelig";

    public string? OwnerName { get; set; }

    public string? ImagePath { get; set; }

    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public bool CanManage { get; set; }

}