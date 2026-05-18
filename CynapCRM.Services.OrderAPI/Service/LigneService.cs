using AutoMapper;
using CynapCRM.Services.OrderAPI.Data;
using CynapCRM.Services.OrderAPI.Models;
using CynapCRM.Services.OrderAPI.Models.Dto;
using CynapCRM.Services.OrderAPI.Service.IService;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace CynapCRM.Services.OrderAPI.Service
{
    public class LigneService : ILigneService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        public LigneService(IMapper mapper, AppDbContext db)
        {
            _mapper = mapper;
            _db = db;
        }
        public async Task<LigneCommandeDto?> CreateOrUpdateLigneCommandeAsync(CreateOrUpdateLigneCommandeDto ligneDto)
        {

            var commande = await _db.Commandes
                            .Include(c => c.Lignes)
                            .FirstOrDefaultAsync(c => c.Id_Commande == ligneDto.Id_Commande);


            if (commande == null || ligneDto.Quantite <= 0)
                return null;

            if (commande.Statut != EtatCommande.Brouillon &&
                commande.Statut != EtatCommande.EnAttente)
                return null;

            LigneCommande ligne;

            if (ligneDto.Id_Ligne == 0)
            {
                ligne = _mapper.Map<LigneCommande>(ligneDto);

                ligne.Commande = commande;        
                ligne.NumeroLot = null;

                commande.Lignes.Add(ligne);
            }
            else
            {
                ligne = commande.Lignes.FirstOrDefault(l => l.Id_Ligne == ligneDto.Id_Ligne);

                if (ligne == null)
                    return null;

                _mapper.Map(ligneDto, ligne);
            }

            // Recalcul du montant HT
            commande.MontantTotalHT = commande.Lignes.Sum(l =>
                (l.PrixUnitaire * l.Quantite) * (1 - (l.Remise / 100)));

            commande.MontantTTC = commande.MontantTotalHT *
                (1 + CreateOrderDto.TauxTVA);

            await _db.SaveChangesAsync();
            return _mapper.Map<LigneCommandeDto>(ligne);

        }

        public async Task<bool> RemoveLigneCommandeAsync(int ligneId)
        {
            var ligne = await _db.LignesCommandes
                .Include(l => l.Commande)
                    .ThenInclude(c => c.Lignes)
                .FirstOrDefaultAsync(l => l.Id_Ligne == ligneId);

            if (ligne == null)
                return false;

            var commande = ligne.Commande;

            if (commande == null ||
                (commande.Statut != EtatCommande.Brouillon &&
                 commande.Statut != EtatCommande.EnAttente))
                return false;

            // FIX: retirer de la collection EN MÉMOIRE avant de recalculer
            commande.Lignes.Remove(ligne);
            _db.LignesCommandes.Remove(ligne);

            // FIX: recalcul propre — la ligne supprimée n'est plus dans la collection
            commande.MontantTotalHT = commande.Lignes.Sum(l =>
                (l.PrixUnitaire * l.Quantite) * (1 - (l.Remise / 100)));

            commande.MontantTTC = commande.MontantTotalHT *
                (1 + CreateOrderDto.TauxTVA);

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
