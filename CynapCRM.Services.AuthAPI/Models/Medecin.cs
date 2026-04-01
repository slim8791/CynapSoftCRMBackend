using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models
{
    public class Medecin : Utilisateur
    {
        public string Specialite { get; set; } = string.Empty;
        public string TypeEtablissement { get; set; } = string.Empty;
    }
}
