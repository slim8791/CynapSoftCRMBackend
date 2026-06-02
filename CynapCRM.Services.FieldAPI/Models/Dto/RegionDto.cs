namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class RegionDto
    {
        public int Id_Region { get; set; }

        public string NomRegion { get; set; } = string.Empty;

        public string CodePostal { get; set; } = string.Empty;

        public int? Id_Superviseur { get; set; }
    }
}
