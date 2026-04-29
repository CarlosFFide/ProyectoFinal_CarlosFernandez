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
        private const int PageSize = 5;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Inicio(string? search, int? creditos, int page = 1)
        {
            var resultado = await ObtenerCursosFiltrados(search, creditos, page);
            CargarDatosPaginacion(resultado.Page, resultado.TotalPages, search, creditos);

            return View(resultado.Cursos);
        }

        public async Task<IActionResult> TablaCursos(string? search, int? creditos, int page = 1)
        {
            var resultado = await ObtenerCursosFiltrados(search, creditos, page);
            CargarDatosPaginacion(resultado.Page, resultado.TotalPages, search, creditos);

            return PartialView("_CursosTabla", resultado.Cursos);
        }

        private async Task<(List<Curso> Cursos, int Page, int TotalPages)> ObtenerCursosFiltrados(string? search, int? creditos, int page)
        {
            var query = _context.Cursos
                .Include(c => c.Carrera)
                .Include(c => c.Docente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Codigo.Contains(search) ||
                    c.Nombre.Contains(search) ||
                    (c.Carrera != null && c.Carrera.Nombre.Contains(search)) ||
                    (c.Docente != null && c.Docente.Nombre.Contains(search)));
            }

            if (creditos.HasValue)
            {
                query = query.Where(c => c.Creditos == creditos.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            var cursos = await query
                .OrderBy(c => c.Codigo)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return (cursos, page, totalPages);
        }

        private void CargarDatosPaginacion(int page, int totalPages, string? search, int? creditos)
        {
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search ?? string.Empty;
            ViewBag.Creditos = creditos;
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

            if (curso == null)
            {
                return NotFound();
            }

            CargarSelectLists(curso.CarreraId, curso.DocenteId);
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

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

            if (curso == null)
            {
                return NotFound();
            }

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