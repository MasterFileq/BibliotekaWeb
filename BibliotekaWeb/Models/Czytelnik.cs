using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BibliotekaWeb.Models
{
    public class Czytelnik
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane.")]
        [Display(Name = "Imię")]
        public string Imie { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [Display(Name = "Nazwisko")]
        public string Nazwisko { get; set; }

        [Required(ErrorMessage = "Numer karty jest wymagany.")]
        [Display(Name = "Numer Karty")]
        public string NumerKarty { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
        public string? KsiazkaTytul { get; set; }
        public DateTime? TerminZwrotu { get; set; }
    }
}