namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class CommandeDto
    {
        public int Id_Commande { get; set; }
        public DateTime DateCommande { get; set; }
        public decimal MontantTotalHT { get; set; }
        public decimal MontantTTC { get; set; }
        public string Statut { get; set; } // On transforme l'Enum en string pour le Frontend
        public int Id_Client { get; set; }

        // Liste des lignes associées
        public List<LigneCommandeDto> Lignes { get; set; } = new();
    }
}
