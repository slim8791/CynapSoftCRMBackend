namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class CommandeDto
    {
        public int Id_Commande { get; set; }
        public DateTime DateCommande { get; set; }
        public decimal MontantTotalHT { get; set; }
        public decimal MontantTTC { get; set; }

        // FIX: EtatCommande au lieu de string pour typage fort
        public EtatCommande Statut { get; set; }

        public int Id_Client { get; set; }

        // FIX: champs manquants
        public string? MotifAnnulation { get; set; }
        public bool IsDeleted { get; set; }

        public List<LigneCommandeDto> Lignes { get; set; } = new();

        // FIX: réclamations manquantes
        public List<ReclamationDto>? Reclamations { get; set; }
    }
}