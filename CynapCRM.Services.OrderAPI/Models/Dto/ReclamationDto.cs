namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class ReclamationDto
    {
        public int Id_Rec { get; set; }

        // FIX: Required
        public string Message { get; set; } = string.Empty;

        public DateTime DateReclamation { get; set; }

        // FIX: StatutReclamation au lieu de string? pour typage fort
        public StatutReclamation Statut { get; set; } = StatutReclamation.Ouverte;

        public int Id_Commande { get; set; }
        public int Id_Ligne { get; set; }
        public int Id_Client { get; set; }
    }
}