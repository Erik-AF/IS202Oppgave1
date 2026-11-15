namespace _202Oppgave.ViewModels;

public class HomeViewModel
{
    
    public int ActiveNeedCount { get; set; }

    public List<string> ActiveNeedTypes { get; set; } = new();

    public string SummaryText
    {
        get
        {
            if (ActiveNeedCount == 0)
                return "Ingen registrerte behov akkurat nå.";

            var types = ActiveNeedTypes
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            if (types.Count == 0)
                return "Registrerte behov krever oppmerksomhet nå.";

            if (types.Count == 1)
                return $"{types[0]} krever oppmerksomhet nå.";

            var summary = string.Join(", ", types.Take(types.Count - 1))
                          + " og " + types.Last();

            return $"{summary} krever oppmerksomhet nå.";
        }
    }
}
