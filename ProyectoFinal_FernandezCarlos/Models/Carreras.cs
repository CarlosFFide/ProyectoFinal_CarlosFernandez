using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_FernandezCarlos.Models
{
    public class Carrera
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código de la carrera es obligatorio.")]
        [StringLength(20, ErrorMessage = "El código de la carrera no puede superar los 20 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la carrera es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la carrera no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
        public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
    }
}