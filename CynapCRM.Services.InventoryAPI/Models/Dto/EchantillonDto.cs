namespace CynapCRM.Services.InventoryAPI.Models.Dto
{
    public class EchantillonDto
    {
        public int Id_Distribution { get; set; }

        public int Id_Delegue { get; set; }

        public int? Id_Medecin { get; set; }

        public int? Id_Pharmacien { get; set; }

        public int Id_Stock { get; set; }

        // Résolu depuis le stock délégué (Echantillon.Id_Stock → Stock_Delegue.Id_Produit).
        // Permet au client d'afficher le nom du produit distribué.
        public int Id_Produit { get; set; }

        public int Qte { get; set; }

        public string NumeroLot { get; set; }

        /// <summary>
        /// Set by the mobile client to the moment of distribution.
        /// Defaults to UTC now so that old clients that omit or send null
        /// never fail model binding — the controller overwrites this with
        /// DateTime.UtcNow anyway.
        /// </summary>
        public DateTime DateDistribution { get; set; } = DateTime.UtcNow;
    }
}
