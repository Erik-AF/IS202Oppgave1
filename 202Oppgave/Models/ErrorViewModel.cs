namespace _202Oppgave.Models;

/// <summary>Formidler forespørsels-ID slik at en feil kan knyttes til serverloggen.</summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
