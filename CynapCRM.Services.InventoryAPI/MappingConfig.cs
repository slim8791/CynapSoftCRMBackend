using AutoMapper;
using CynapCRM.Services.InventoryAPI.Models;
using CynapCRM.Services.InventoryAPI.Models.Dto;



namespace CynapCRM.Services.InventoryAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<StockDelegueDto, Stock_Delegue>().ReverseMap();
            CreateMap<StockEchantillonDto, Stock_Echantillon>().ReverseMap();
            CreateMap<StockGratuiteDto, Stock_Gratuite>().ReverseMap();
            CreateMap<EchantillonDto, Echantillon>().ReverseMap();
            CreateMap<StockMovementDto, StockMovement>().ReverseMap();
        }
    }
}
