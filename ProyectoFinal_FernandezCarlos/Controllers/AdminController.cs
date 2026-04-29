using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoFinal_FernandezCarlos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        public IActionResult Inicio()
        {
            return View();
        }
    }
}