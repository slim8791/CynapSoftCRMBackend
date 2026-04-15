namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class RegionDto
    {
        public int Id_Region { get; set; }

        public string NomRegion { get; set; } = string.Empty;

        public int CodePostal { get; set; }

        public int Id_User_Delegue { get; set; }
    }
}
