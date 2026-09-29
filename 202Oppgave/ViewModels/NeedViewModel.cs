using System.ComponentModel.DataAnnotations;

namespace _202Oppgave.ViewModels
{
    public class NeedViewModel
    {
        [Required(ErrorMessage = "Oppgi hvilken type ressurs det er behov for.")]
        public string Type { get; set; } = string.Empty;


        [Required(ErrorMessage = "Oppgi hvor hjelpen trengs.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Velg hvor mye behovet haster.")]
        [EnumDataType(typeof(NeedPriority))]
        public NeedPriority? Priority { get; set; }

        [Required(ErrorMessage = "Beskriv hva det er behov for.")]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Oppgi når hjelpen trengs.")]
        public DateTime? Deadline { get; set; }


        [Required(ErrorMessage = "Velg et punkt på kartet.")]
        [Range(-90, 90, ErrorMessage = "Ugyldig breddegrad.")]
        public double? Latitude { get; set; }

        [Required(ErrorMessage = "Velg et punkt på kartet.")]
        [Range(-180, 180, ErrorMessage = "Ugyldig lengdegrad.")]
        public double? Longitude { get; set; }
    }
    public enum NeedPriority
    {
        IkkeAkutt = 1,
        Akutt = 2
    }
}


