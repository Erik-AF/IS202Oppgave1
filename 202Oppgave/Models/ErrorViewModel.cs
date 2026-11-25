namespace _202Oppgave.Models;

/// Formidler forespørsels-ID slik at en feil kan knyttes til serverloggen.
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
