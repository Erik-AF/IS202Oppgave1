namespace _202Oppgave.Models.Entities
{

    /// Knytter én ressurs til ett behov og lagrer status, tidspunkt og eventuell merknad.
    public class Allocation
    {

        public int AllocationId { get; set; }

        public string State { get; set; } = "Forespurt";

        public DateTime Date { get; set; }

        public string? Note { get; set; }

        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public int NeedId { get; set; }
        public Need Need { get; set; } = null!;
    }
}