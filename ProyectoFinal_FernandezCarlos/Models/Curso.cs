using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_FernandezCarlos.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El código del curso es obligatorio.")]
        [StringLength(20, ErrorMessage = "El código del curso no puede superar los 20 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del curso no puede superar los 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los créditos del curso son obligatorios.")]
        [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10.")]
        [Display(Name = "Créditos")]
        public int? Creditos { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una carrera para el curso.")]
        [Display(Name = "Carrera")]
        public int? CarreraId { get; set; }

        public Carrera? Carrera { get; set; }

        [Display(Name = "Docente")]
        public int? DocenteId { get; set; }

        public Docente? Docente { get; set; }

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}