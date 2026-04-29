using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal_FernandezCarlos.Data;
using ProyectoFinal_FernandezCarlos.Models;

namespace ProyectoFinal_FernandezCarlos.Controllers
{
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int PageSize = 5;

        public EstudianteController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Inicio()
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            return View(estudiante);
        }

        public async Task<IActionResult> Carreras(int page = 1)
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            var query = _context.Carreras
                .OrderBy(c => c.Nombre)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = CalcularTotalPaginas(totalItems);

            page = AjustarPagina(page, totalPages);

            var carreras = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CarreraActualId = estudiante.CarreraId;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(carreras);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeleccionarCarrera(int carreraId)
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            var carrera = await _context.Carreras.FindAsync(carreraId);

            if (carrera == null)
            {
                return NotFound();
            }

            estudiante.CarreraId = carreraId;

            var matriculasFueraDeCarrera = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.EstudianteId == estudiante.Id && m.Curso != null && m.Curso.CarreraId != carreraId)
                .ToListAsync();

            if (matriculasFueraDeCarrera.Any())
            {
                _context.Matriculas.RemoveRange(matriculasFueraDeCarrera);
            }

            _context.Estudiantes.Update(estudiante);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Carrera seleccionada correctamente.";

            return RedirectToAction(nameof(Inicio));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarCarrera()
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            var matriculas = await _context.Matriculas
                .Where(m => m.EstudianteId == estudiante.Id)
                .ToListAsync();

            if (matriculas.Any())
            {
                _context.Matriculas.RemoveRange(matriculas);
            }

            estudiante.CarreraId = null;

            _context.Estudiantes.Update(estudiante);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "La carrera seleccionada fue removida correctamente.";

            return RedirectToAction(nameof(Inicio));
        }

        public async Task<IActionResult> CursosDisponibles(int page = 1)
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            if (estudiante.CarreraId == null)
            {
                TempData["Error"] = "Debe seleccionar una carrera antes de matricular cursos.";
                return RedirectToAction(nameof(Carreras));
            }

            var query = _context.Cursos
                .Include(c => c.Carrera)
                .Include(c => c.Docente)
                .Where(c => c.CarreraId == estudiante.CarreraId)
                .OrderBy(c => c.Codigo)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = CalcularTotalPaginas(totalItems);

            page = AjustarPagina(page, totalPages);

            var cursos = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var cursosMatriculados = await _context.Matriculas
                .Where(m => m.EstudianteId == estudiante.Id)
                .Select(m => m.CursoId)
                .ToListAsync();

            ViewBag.CursosMatriculados = cursosMatriculados;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(cursos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Matricular(int cursoId)
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            if (estudiante.CarreraId == null)
            {
                TempData["Error"] = "Debe seleccionar una carrera antes de matricular cursos.";
                return RedirectToAction(nameof(Carreras));
            }

            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId);

            if (curso == null)
            {
                return NotFound();
            }

            if (curso.CarreraId != estudiante.CarreraId)
            {
                TempData["Error"] = "No puede matricular un curso que no pertenece a su carrera seleccionada.";
                return RedirectToAction(nameof(CursosDisponibles));
            }

            var yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.EstudianteId == estudiante.Id && m.CursoId == cursoId);

            if (yaMatriculado)
            {
                TempData["Error"] = "El curso ya se encuentra matriculado.";
                return RedirectToAction(nameof(CursosDisponibles));
            }

            var matricula = new Matricula
            {
                EstudianteId = estudiante.Id,
                CursoId = cursoId,
                FechaMatricula = DateTime.Now
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Curso matriculado correctamente.";

            return RedirectToAction(nameof(CursosDisponibles));
        }

        public async Task<IActionResult> MisCursos(int page = 1)
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            var query = _context.Matriculas
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Carrera)
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Docente)
                .Where(m => m.EstudianteId == estudiante.Id)
                .OrderBy(m => m.Curso!.Codigo)
                .AsQueryable();

            var totalItems = await query.CountAsync();
            var totalPages = CalcularTotalPaginas(totalItems);

            page = AjustarPagina(page, totalPages);

            var matriculas = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(matriculas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retirar(int matriculaId)
        {
            var estudiante = await ObtenerEstudianteActual();

            if (estudiante == null)
            {
                return NotFound();
            }

            var matricula = await _context.Matriculas
                .FirstOrDefaultAsync(m => m.Id == matriculaId && m.EstudianteId == estudiante.Id);

            if (matricula == null)
            {
                TempData["Error"] = "No se encontró la matrícula seleccionada.";
                return RedirectToAction(nameof(MisCursos));
            }

            _context.Matriculas.Remove(matricula);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Curso retirado correctamente.";

            return RedirectToAction(nameof(MisCursos));
        }

        private async Task<Estudiante?> ObtenerEstudianteActual()
        {
            var userId = _userManager.GetUserId(User);

            return await _context.Estudiantes
                .Include(e => e.Carrera)
                .FirstOrDefaultAsync(e => e.UserId == userId);
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