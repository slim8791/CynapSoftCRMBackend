using AutoMapper;
using CynapCRM.Services.ProductAPI.Models;
using CynapCRM.Services.ProductAPI.Models.Dto;

namespace CynapCRM.Services.ProductAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            // Produit
            CreateMap<Produit, ProduitDto>();
            CreateMap<ProduitDto, Produit>()
                .ForMember(dest => dest.Lots, opt => opt.Ignore())
                .ForMember(dest => dest.Supports, opt => opt.Ignore());

            // Lot
            CreateMap<Lot, LotDto>()
                .ForMember(dest => dest.Numero, 
                           opt => opt.MapFrom(src => src.NumeroLot))
                .ForMember(dest => dest.IsExpired,
                           opt => opt.MapFrom(src =>
                               src.DateExpiration < DateTime.UtcNow))
                .ForMember(dest => dest.IsOutOfStock,
                           opt => opt.MapFrom(src => src.Quantite <= 0));
            CreateMap<LotDto, Lot>()
                .ForMember(dest => dest.NumeroLot, 
                           opt => opt.MapFrom(src => src.Numero))
                .ForMember(dest => dest.Promotions, opt => opt.Ignore())
                .ForMember(dest => dest.Produit, opt => opt.Ignore());

            // Promotion
            CreateMap<Promotion, PromotionDto>()
                .ForMember(dest => dest.IsValid,
                           opt => opt.MapFrom(src =>
                               src.EstActive &&
                               src.DateExpiration > DateTime.UtcNow &&
                               src.DateDebut != null &&
                               src.DateDebut <= DateTime.UtcNow));
            CreateMap<PromotionDto, Promotion>()
                .ForMember(dest => dest.Lot, opt => opt.Ignore());

            // Support
            CreateMap<Support_Marketting, SupportMarketingDto>();
            CreateMap<SupportMarketingDto, Support_Marketting>()
                .ForMember(dest => dest.Fichiers, opt => opt.Ignore())
                .ForMember(dest => dest.Produit, opt => opt.Ignore());

            // Fichier
            CreateMap<Fichier, FichierDto>().ReverseMap();
        }
    }
}