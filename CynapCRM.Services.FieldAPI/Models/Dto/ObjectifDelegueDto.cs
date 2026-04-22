using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class ObjectifDelegueDto
    {


        public int Id_Objectif { get; set; }

        [Required]
        public TypeObjectif Type { get; set; }   

        [Required]
        public int ValeurCible { get; set; }

        public int ValeurRealisee { get; set; }

        [Required]
        public PeriodeObjectif Periode { get; set; }

        // Injecté depuis le JWT (Claim‑Based Identity)
        public int Id_User_Delegue { get; set; }


    }
}
