using System.ComponentModel.DataAnnotations;

namespace CynapCRM.Services.AuthAPI.Models
{
    public class Pharmacien : Client
    {
        public string NomOfficine { get; set; } = string.Empty;

        public string TypePharmacie { get; set; } = string.Empty;
    }
}
