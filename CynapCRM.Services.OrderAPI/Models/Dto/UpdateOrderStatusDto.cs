namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class UpdateOrderStatusDto
    {
        public int Id_Commande { get; set; }
        public EtatCommande NouveauStatut { get; set; } // On utilise l'index de l'Enum EtatCommande
    }
}
