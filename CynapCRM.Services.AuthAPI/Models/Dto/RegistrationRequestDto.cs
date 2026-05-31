namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class RegistrationRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;

        // Pharmacien
        public string? NomOfficine { get; set; }
        public string? TypePharmacie { get; set; }

        // Grossiste
        public string? RaisonSociale { get; set; }


        public UserRole Role { get; set; }      // CLIENT / ADMIN / SUPERVISEUR / DELEGUE / MEDECIN
        public UserType UserType { get; set; }  // PHARMACIEN / GROSSISTE
        public int? IdRegion { get; set; }
    }
}
