using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_FernandezCarlos.Models
{
    public class Docente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del docente es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del docente no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo del docente es obligatorio.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido.")]
        [Display(Name = "Correo")]
        public string Correo { get; set; } = string.Empty;

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
    }
}