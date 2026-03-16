using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_FernandezCarlos.Data;
using ProyectoFinal_FernandezCarlos.Models;

namespace ProyectoFinal_FernandezCarlos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CarrerasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CarrerasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Inicio()
        {
            var carreras = await _context.Set<Carrera>().ToListAsync();
            return View(carreras);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Carrera carrera)
        {
            if (!ModelState.IsValid)
            {
                return View(carrera);
            }

            _context.Set<Carrera>().Add(carrera);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var carrera = await _context.Carreras.FindAsync(id);
            if (carrera == null) return NotFound();
            return View(carrera);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Carrera carrera)
        {
            if (id != carrera.Id) return NotFound();
            if (!ModelState.IsValid) return View(carrera);

            _context.Carreras.Update(carrera);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var carrera = await _context.Carreras.FindAsync(id);
            if (carrera == null) return NotFound();
            return View(carrera);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var carrera = await _context.Carreras.FindAsync(id);
            if (carrera != null)
            {
                _context.Carreras.Remove(carrera);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Inicio));
        }
    }
}