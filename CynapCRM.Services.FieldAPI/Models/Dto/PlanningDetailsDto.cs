namespace CynapCRM.Services.FieldAPI.Models.Dto
{
    public class PlanningDetailsDto
    {
        public PlanningVisiteDto Planning { get; set; }

        public List<TourneeDto> Tournees { get; set; } = new();
    }
}
