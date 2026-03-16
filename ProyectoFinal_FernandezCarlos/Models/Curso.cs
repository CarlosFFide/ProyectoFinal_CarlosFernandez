using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal_FernandezCarlos.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Creditos { get; set; }

        public int CarreraId { get; set; }
        public Carrera? Carrera { get; set; }

        public int? DocenteId { get; set; }
        public Docente? Docente { get; set; }

        public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    }
}