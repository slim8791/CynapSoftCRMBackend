using AutoMapper;
using CynapCRM.Services.DocAPI.Models;
using CynapCRM.Services.DocAPI.Models.Dto;



namespace CynapCRM.Services.DocAPI
{
    public class MappingConfig : Profile
    {
        public MappingConfig() 
        {
            CreateMap<DocumentDto, Document>()
    .ForMember(d => d.Numero_Doc, opt => opt.MapFrom(src => src.Numero_Doc))
    .ForMember(d => d.Nom_Doc, opt => opt.MapFrom(src => src.Nom_Doc))
    .ForMember(d => d.DateCreation, opt => opt.MapFrom(src => src.DateCreation))
    .ForMember(d => d.Id_Commande, opt => opt.MapFrom(src => src.Id_Commande))
    .ForMember(d => d.Id_Client, opt => opt.MapFrom(src => src.Id_Client))
    .ForMember(d => d.IsDeleted, opt => opt.Ignore());


            CreateMap<Document, DocumentDto>();
            CreateMap<BonCommandeDto, BonCommande>();


            CreateMap<BonCommande, BonCommandeDto>();

            CreateMap<BonLivraisonDto, BonLivraison>()
                .IncludeBase<DocumentDto, Document>();

            CreateMap<BonLivraison, BonLivraisonDto>()
                .IncludeBase<Document, DocumentDto>();

            CreateMap<FactureDto, Facture>()
                .IncludeBase<DocumentDto, Document>();

            CreateMap<Facture, FactureDto>()
                .IncludeBase<Document, DocumentDto>();

        }
    }
}
