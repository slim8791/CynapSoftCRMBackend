namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class TourneeDto
    {
        public int Id_Tournee { get; set; }

        public DateTime Date { get; set; }

        public string Statut { get; set; } = string.Empty;

        public int Id_Planning { get; set; }

        public int Id_User_Delegue { get; set; }
    }
}
