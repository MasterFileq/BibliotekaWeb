using Microsoft.AspNetCore.Identity;
using System;

namespace BibliotekaWeb.Models
{
    public class CzytelnikViewModel
    {
        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }
        public string KsiazkaTytul { get; set; }
        public DateTime? TerminZwrotu { get; set; }
    }
}