using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CynapCRM.Services.AuthAPI.Models
{
    public class Utilisateur : IdentityUser<int> //table user identity 
    {

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Adresse { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;



    }
}
