using AutoMapper;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;



namespace CynapCRM.Services.OrderAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<CommandeDto, Commande>().ReverseMap();
            CreateMap<LigneCommandeDto, LigneCommande>().ReverseMap();
            CreateMap<ReclamationDto, Reclamation>().ReverseMap();
            CreateMap<CreateOrderDto, Commande>().ReverseMap();

            CreateMap<CreateOrUpdateLigneCommandeDto, LigneCommande>()
                .ForMember(dest => dest.NumeroLot, opt => opt.Condition(src => src.NumeroLot != null));


        }
    }
}
