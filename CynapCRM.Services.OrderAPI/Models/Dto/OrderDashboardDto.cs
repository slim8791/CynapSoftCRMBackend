namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class OrderDashboardDto
    {
        // Volumes par statut
        public int TotalCommandes { get; set; }
        public int EnAttente { get; set; }
        public int Confirmees { get; set; }
        public int EnPreparation { get; set; }
        public int Expediees { get; set; }
        public int Livrees { get; set; }
        public int Annulees { get; set; }

        // Financier — commandes livrées uniquement
        public decimal MontantTotalHT { get; set; }
        public decimal MontantTotalTTC { get; set; }

        // Réclamations
        public int ReclamationsOuvertes { get; set; }
        public int ReclamationsEnCours { get; set; }
        public int ReclamationsResolues { get; set; }

        // Tendance
        public int CommandesAujourdHui { get; set; }
        public int CommandesCeMois { get; set; }
    }
}