namespace _202Oppgave.Models.Entities
{
    // Allocation kobler en ressurs til et behov (koblingstabell i ER-diagrammet).
    // Én ressurs kan ha mange allokeringer, og ett behov kan ha mange allokeringer.
    public class Allocation
    {
        // Primærnøkkel. Heter AllocationId slik som i ER-diagrammet.
        public int AllocationId { get; set; }

        // Hvor langt allokeringen har kommet: Forespurt, Bekreftet, Fullført eller Avbrutt
        public string State { get; set; } = "Forespurt";

        // Når allokeringen ble laget. Settes automatisk av databasen.
        public DateTime Date { get; set; }

        // Valgfri merknad, f.eks. "Hentes ved låven kl. 08"
        public string? Note { get; set; }

        // Fremmednøkkel til Resources, og selve ressursen
        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        // Fremmednøkkel til Needs, og selve behovet
        public int NeedId { get; set; }
        public Need Need { get; set; } = null!;
    }
}