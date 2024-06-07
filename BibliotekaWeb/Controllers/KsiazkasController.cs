using BibliotekaWeb.Data;
using BibliotekaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaWeb.Controllers
{
    [Authorize]
    public class KsiazkasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        //private readonly RoleManager<IdentityRole> _roleManager;

        public KsiazkasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Administrator, Bibliotekarz, Czytelnik")]
        public async Task<IActionResult> Index(string searchString, string author, string isbn, bool? available)
        {
            var books = from b in _context.Ksiazka
                        select b;

            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Tytul.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => b.Autor.Contains(author));
            }

            if (!string.IsNullOrEmpty(isbn))
            {
                books = books.Where(b => b.ISBN.Contains(isbn));
            }

            if (available.HasValue)
            {
                books = books.Where(b => b.Dostepnosc == available.Value);
            }

            return View(await books.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ksiazka = await _context.Ksiazka
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ksiazka == null)
            {
                return NotFound();
            }

            return View(ksiazka);
        }



        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> Create([Bind("Id,Tytul,Autor,ISBN,Dostepnosc")] Ksiazka ksiazka)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ksiazka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ksiazka);
        }

        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ksiazka = await _context.Ksiazka.FindAsync(id);
            if (ksiazka == null)
            {
                return NotFound();
            }
            return View(ksiazka);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tytul,Autor,ISBN,Dostepnosc")] Ksiazka ksiazka)
        {
            if (id != ksiazka.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ksiazka);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KsiazkaExists(ksiazka.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ksiazka);
        }

        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ksiazka = await _context.Ksiazka
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ksiazka == null)
            {
                return NotFound();
            }

            return View(ksiazka);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ksiazka = await _context.Ksiazka.FindAsync(id);
            if (ksiazka != null)
            {
                _context.Ksiazka.Remove(ksiazka);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool KsiazkaExists(int id)
        {
            return _context.Ksiazka.Any(e => e.Id == id);
        }

        [Authorize(Roles = "Administrator, Bibliotekarz, Czytelnik")]
        public async Task<IActionResult> Wypozycz(int id)
        {
            var ksiazka = await _context.Ksiazka.FindAsync(id);
            if (ksiazka == null)
            {
                return NotFound();
            }

            if (!ksiazka.Dostepnosc)
            {
                TempData["ErrorMessage"] = "Książka jest już wypożyczona.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Nie udało się znaleźć zalogowanego użytkownika.";
                return RedirectToAction(nameof(Index));
            }

            var czytelnikId = user.Id;

            var wypozyczenie = new Wypozyczenie
            {
                KsiazkaId = id,
                CzytelnikId = czytelnikId,
                DataWypozyczenia = DateTime.Now,
                TerminZwrotu = DateTime.Now.AddDays(14)
            };

            ksiazka.Dostepnosc = false;

            try
            {
                _context.Add(wypozyczenie);
                _context.Update(ksiazka);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas wypożyczania książki: {ex.Message} InnerException: {ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Książka została pomyślnie wypożyczona.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> Zwroc(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Nie udało się znaleźć zalogowanego użytkownika.";
                return RedirectToAction(nameof(Index));
            }

            var czytelnikId = user.Id;

            var wypozyczenie = await _context.Wypozyczenie
                .FirstOrDefaultAsync(w => w.KsiazkaId == id && w.CzytelnikId == czytelnikId);

            if (wypozyczenie == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wypożyczenia dla tej książki.";
                return RedirectToAction(nameof(Index));
            }

            var ksiazka = await _context.Ksiazka.FindAsync(id);
            if (ksiazka == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono książki.";
                return RedirectToAction(nameof(Index));
            }

            ksiazka.Dostepnosc = true;

            try
            {
                _context.Wypozyczenie.Remove(wypozyczenie);
                _context.Update(ksiazka);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas zwracania książki: {ex.Message} InnerException: {ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Książka została pomyślnie zwrócona.";
            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> ZwrocByAdmin(int id)
        {
            var wypozyczenie = await _context.Wypozyczenie
                .FirstOrDefaultAsync(w => w.KsiazkaId == id);

            if (wypozyczenie == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono wypożyczenia dla tej książki.";
                return RedirectToAction(nameof(Index));
            }

            var ksiazka = await _context.Ksiazka.FindAsync(id);
            if (ksiazka == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono książki.";
                return RedirectToAction(nameof(Index));
            }

            ksiazka.Dostepnosc = true;

            try
            {
                _context.Wypozyczenie.Remove(wypozyczenie);
                _context.Update(ksiazka);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = $"Wystąpił błąd podczas zwracania książki: {ex.Message} InnerException: {ex.InnerException?.Message}";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Książka została pomyślnie zwrócona.";
            return RedirectToAction(nameof(Index));
        }
    }
}
