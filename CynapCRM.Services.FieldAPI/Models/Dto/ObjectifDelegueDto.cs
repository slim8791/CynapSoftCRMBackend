namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class ObjectifDelegueDto
    {
        public int Id_Objectif { get; set; }

        public string Type { get; set; } = string.Empty;

        public int ValeurCible { get; set; }

        public string Periode { get; set; } = string.Empty;

        public int Id_User_Delegue { get; set; }
    }
}
