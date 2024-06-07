using System.ComponentModel.DataAnnotations;

namespace BibliotekaWeb.Models
{
    public class Ksiazka
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        [Display(Name = "Tytuł")]
        public string Tytul { get; set; }

        [Required(ErrorMessage = "Autor jest wymagany.")]
        [Display(Name = "Autor")]
        public string Autor { get; set; }

        [Required(ErrorMessage = "ISBN jest wymagany.")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN musi mieć od 10 do 13 znaków.")]
        [Display(Name = "ISBN")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Dostępność jest wymagana.")]
        [Display(Name = "Dostępność")]
        public bool Dostepnosc { get; set; }
    }
}