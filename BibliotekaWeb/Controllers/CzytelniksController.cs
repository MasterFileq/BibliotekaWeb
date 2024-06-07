using BibliotekaWeb.Data;
using BibliotekaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotekaWeb.Controllers
{
    [Authorize]
    public class CzytelniksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CzytelniksController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> Index()
        {
            var czytelnicy = await _userManager.GetUsersInRoleAsync("Czytelnik");

            var czytelnikViewModels = new List<CzytelnikViewModel>();

            foreach (var user in czytelnicy)
            {
                var wypozyczenie = await _context.Wypozyczenie
                    .Include(w => w.Ksiazka)
                    .Where(w => w.CzytelnikId == user.Id)
                    .OrderByDescending(w => w.DataWypozyczenia)
                    .FirstOrDefaultAsync();

                czytelnikViewModels.Add(new CzytelnikViewModel
                {
                    User = user,
                    KsiazkaTytul = wypozyczenie?.Ksiazka?.Tytul,
                    TerminZwrotu = wypozyczenie?.TerminZwrotu
                });
            }

            return View(czytelnikViewModels);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(string id, IdentityUser model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Delete", user);
            }

            return RedirectToAction(nameof(Index));
        }




        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> AssignBook(string czytelnikId)
        {
            var ksiazki = await _context.Ksiazka.Where(k => k.Dostepnosc).ToListAsync();
            var czytelnik = await _userManager.FindByIdAsync(czytelnikId);

            if (czytelnik == null)
            {
                TempData["ErrorMessage"] = "Nie znaleziono czytelnika.";
                return RedirectToAction(nameof(Index));
            }

            var model = new WypozyczenieViewModel
            {
                CzytelnikId = czytelnikId,
                Email = czytelnik.Email,
                Ksiazki = ksiazki
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Bibliotekarz")]
        public async Task<IActionResult> AssignBook(WypozyczenieViewModel model)
        {
            if (ModelState.IsValid)
            {
                var czytelnik = await _userManager.FindByIdAsync(model.CzytelnikId);
                if (czytelnik == null)
                {
                    TempData["ErrorMessage"] = "Nie znaleziono czytelnika z podanym identyfikatorem.";
                    model.Ksiazki = await _context.Ksiazka.Where(k => k.Dostepnosc).ToListAsync();
                    return View(model);
                }

                var ksiazka = await _context.Ksiazka.FindAsync(model.KsiazkaId);
                if (ksiazka == null || !ksiazka.Dostepnosc)
                {
                    TempData["ErrorMessage"] = "Wybrana książka jest niedostępna.";
                    model.Ksiazki = await _context.Ksiazka.Where(k => k.Dostepnosc).ToListAsync();
                    return View(model);
                }

                var wypozyczenie = new Wypozyczenie
                {
                    KsiazkaId = model.KsiazkaId,
                    CzytelnikId = model.CzytelnikId,
                    DataWypozyczenia = DateTime.Now,
                    TerminZwrotu = DateTime.Now.AddDays(14)
                };

                _context.Add(wypozyczenie);

                ksiazka.Dostepnosc = false;
                _context.Update(ksiazka);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["ErrorMessage"] = string.Join(", ", errorMessages);
            model.Ksiazki = await _context.Ksiazka.Where(k => k.Dostepnosc).ToListAsync();
            return View(model);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Czytelnik"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Czytelnik"));
                    }
                    await _userManager.AddToRoleAsync(user, "Czytelnik");

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
