namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class CommandeDto
    {
        public int Id_Commande { get; set; }
        public DateTime DateCommande { get; set; }
        public decimal MontantTotalHT { get; set; }
        public decimal MontantTTC { get; set; }
        public string Statut { get; set; } 
        public int Id_Client { get; set; }

        public List<LigneCommandeDto> Lignes { get; set; } = new();
    }
}
