using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_FernandezCarlos.Data;
using ProyectoFinal_FernandezCarlos.Models;

namespace ProyectoFinal_FernandezCarlos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DocentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 5;

        public DocentesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Inicio(int page = 1)
        {
            var query = _context.Docentes
                .OrderBy(d => d.Nombre)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = CalcularTotalPaginas(totalItems);

            page = AjustarPagina(page, totalPages);

            var docentes = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(docentes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Docente docente)
        {
            if (!ModelState.IsValid)
            {
                return View(docente);
            }

            _context.Docentes.Add(docente);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var docente = await _context.Docentes.FindAsync(id);

            if (docente == null)
            {
                return NotFound();
            }

            return View(docente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Docente docente)
        {
            if (id != docente.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(docente);
            }

            _context.Docentes.Update(docente);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var docente = await _context.Docentes
                .Include(d => d.Cursos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (docente == null)
            {
                return NotFound();
            }

            return View(docente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var docente = await _context.Docentes
                .Include(d => d.Cursos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (docente != null)
            {
                if (docente.Cursos.Any())
                {
                    TempData["Error"] = "No se puede eliminar el docente porque tiene cursos asociados.";
                    return RedirectToAction(nameof(Inicio));
                }

                _context.Docentes.Remove(docente);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Inicio));
        }

        private int CalcularTotalPaginas(int totalItems)
        {
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            return totalPages == 0 ? 1 : totalPages;
        }

        private int AjustarPagina(int page, int totalPages)
        {
            if (page < 1)
            {
                return 1;
            }

            if (page > totalPages)
            {
                return totalPages;
            }

            return page;
        }
    }
}