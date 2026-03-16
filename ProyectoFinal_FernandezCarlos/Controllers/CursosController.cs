using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_FernandezCarlos.Data;
using ProyectoFinal_FernandezCarlos.Models;

namespace ProyectoFinal_FernandezCarlos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Inicio()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Carrera)
                .Include(c => c.Docente)
                .ToListAsync();
            return View(cursos);
        }

        private void CargarSelectLists(int? carreraId = null, int? docenteId = null)
        {
            ViewBag.CarreraId = new SelectList(_context.Carreras, "Id", "Nombre", carreraId);
            ViewBag.DocenteId = new SelectList(_context.Docentes, "Id", "Nombre", docenteId);
        }

        public IActionResult Create()
        {
            CargarSelectLists();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (!ModelState.IsValid)
            {
                CargarSelectLists(curso.CarreraId, curso.DocenteId);
                return View(curso);
            }
            _context.Cursos.Add(curso);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();
            CargarSelectLists(curso.CarreraId, curso.DocenteId);
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                CargarSelectLists(curso.CarreraId, curso.DocenteId);
                return View(curso);
            }
            _context.Cursos.Update(curso);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Carrera)
                .Include(c => c.Docente)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null) return NotFound();
            return View(curso);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso != null)
            {
                if (curso.Matriculas.Any())
                {
                    TempData["Error"] = "No se puede eliminar el curso porque tiene matrículas asociadas.";
                    return RedirectToAction(nameof(Inicio));
                }
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Inicio));
        }
    }
}
