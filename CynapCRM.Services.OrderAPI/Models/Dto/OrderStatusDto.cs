namespace CynapCRM.Services.OrderAPI.Models.Dto
{
    public class OrderStatusDto
    {
        public int Id_Commande { get; set; }
        public EtatCommande Statut { get; set; }
        public DateTime DateCommande { get; set; }
    }
}
