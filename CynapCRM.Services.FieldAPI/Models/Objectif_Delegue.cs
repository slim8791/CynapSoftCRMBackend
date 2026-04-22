using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models
{
    public class Objectif_Delegue
    {

        [Key]
        public int Id_Objectif { get; set; }

        [Required]
        public TypeObjectif Type { get; set; }

        [Required]
        public int ValeurCible { get; set; }

        public int ValeurRealisee { get; set; } = 0;

        [Required]
        public PeriodeObjectif Periode { get; set; }

        [Required]
        public int Id_User_Delegue { get; set; }

        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }

    }
}
