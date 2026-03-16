using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_FernandezCarlos.Models
{
    public class Carrera
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
        public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
    }
}