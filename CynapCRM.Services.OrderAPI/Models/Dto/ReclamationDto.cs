namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class ReclamationDto
    {
        public int Id_Rec { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DateReclamation { get; set; }
        public string Statut { get; set; }
        public int Id_Commande { get; set; }
        public int Id_Ligne { get; set; }
        public int Id_Client { get; set; }
    }
}
