namespace CynapCRM.Services.OrderAPI.Models
{
    public enum EtatCommande
    {
        Brouillon = 0,      // En cours de saisie 
        EnAttente = 1,      // Envoyée mais pas encore validée 
        Validee = 2,        // Commande confirmée
        Expediee = 3,       // En cours de livraison
        Livree = 4,         // Réceptionnée par le client
        Annulee = 5          // Commande annulée
    }
}
