namespace CynapCRM.Services.OrderAPI.Models
{
    public enum EtatCommande
    {
        Brouillon = 0,  // En cours de saisie
        EnAttente = 1,  // Envoyée, pas encore validée
        Confirmee = 2,  // Commande confirmée par l'admin
        EnPreparation = 3,  // En cours de préparation
        Expediee = 4,  // En cours de livraison
        Livree = 5,  // Réceptionnée par le client
        Annulee = 6   // Commande annulée
    }
}