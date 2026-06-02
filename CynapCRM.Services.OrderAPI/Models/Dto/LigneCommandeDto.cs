namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class LigneCommandeDto
    {
        public int Id_Ligne { get; set; }
        public int Id_Produit { get; set; }
        public int Id_Commande { get; set; }
        public int Quantite { get; set; }
        public decimal Remise { get; set; }
        public decimal PrixUnitaire { get; set; }

        // FIX: nullable — lot assigné plus tard, pas à la création
        public string? NumeroLot { get; set; }

        // FIX: sous-total calculé utile pour l'affichage mobile
        public decimal SousTotal => (PrixUnitaire * Quantite) * (1 - (Remise / 100));
    }
}