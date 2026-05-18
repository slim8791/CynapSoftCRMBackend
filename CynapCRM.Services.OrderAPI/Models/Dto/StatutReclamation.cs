// FIX: déplacer dans Models (pas Dto) pour cohérence avec EtatCommande
namespace CynapCRM.Services.OrderAPI.Models
{
    public enum StatutReclamation
    {
        Ouverte = 0,
        EnCours = 1,
        Resolue = 2
    }
}