using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BibliotekaWeb.Models
{
    public class Wypozyczenie
    {
        public int Id { get; set; }

        [Required]
        public int KsiazkaId { get; set; }

        [Required]
        public string CzytelnikId { get; set; }

        public DateTime DataWypozyczenia { get; set; } = DateTime.Now;

        public DateTime TerminZwrotu { get; set; } = DateTime.Now.AddDays(14);

        public decimal Kara { get; set; } = 0;
        public bool CzyZwrócone { get; set; }

        public virtual Ksiazka Ksiazka { get; set; }
        public string? UserId { get; set; }
        public virtual IdentityUser? Czytelnik { get; set; }
    }
}
