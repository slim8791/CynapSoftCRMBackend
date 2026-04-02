namespace CynapCRM.Services.AuthAPI.Models.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;


    }
}
