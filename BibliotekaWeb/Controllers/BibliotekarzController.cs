using BibliotekaWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BibliotekaWeb.Controllers
{
    [Authorize(Roles = "Administrator, Bibliotekarz, Czytelnik")]
    public class BibliotekarzController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BibliotekarzController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Czytelnik"))
            {
                return RedirectToAction(nameof(HistoriaWypozyczen));
            }

            var wypozyczenia = await _context.Wypozyczenie
                .Include(w => w.Ksiazka)
                .Include(w => w.Czytelnik)
                .ToListAsync();

            foreach (var wypozyczenie in wypozyczenia)
            {
                var overdueDays = (DateTime.Now - wypozyczenie.TerminZwrotu).Days;
                if (overdueDays > 0 && !wypozyczenie.Ksiazka.Dostepnosc)
                {
                    wypozyczenie.Kara = overdueDays * 5;
                }
                else
                {
                    wypozyczenie.Kara = 0;
                }
            }

            await _context.SaveChangesAsync();

            return View(wypozyczenia);
        }

        [Authorize(Roles = "Czytelnik, Administrator, Bibliotekarz")]
        public async Task<IActionResult> HistoriaWypozyczen()
        {
            var userId = _userManager.GetUserId(User);
            var wypozyczenia = await _context.Wypozyczenie
                .Include(w => w.Ksiazka)
                .Include(w => w.Czytelnik)
                .Where(w => w.CzytelnikId == userId || User.IsInRole("Administrator") || User.IsInRole("Bibliotekarz"))
                .ToListAsync();

            return View(wypozyczenia);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsReturned(int id)
        {
            var wypozyczenie = await _context.Wypozyczenie
                .Include(w => w.Ksiazka)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (wypozyczenie == null || wypozyczenie.Ksiazka == null)
            {
                return NotFound();
            }

            wypozyczenie.CzyZwrócone = true;
            wypozyczenie.Ksiazka.Dostepnosc = true;

            _context.Update(wypozyczenie);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
