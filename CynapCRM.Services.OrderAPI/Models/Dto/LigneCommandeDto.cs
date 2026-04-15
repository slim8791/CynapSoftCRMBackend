namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class LigneCommandeDto
    {
        public int Id_Ligne { get; set; }
        public int Id_Produit { get; set; }
        public int Id_Commande { get; set; }
        public int Quantite { get; set; }
        public decimal Remise { get; set; }
        public string NumeroLot { get; set; } = string.Empty;
        public decimal PrixUnitaire { get; set; }
    }
}
