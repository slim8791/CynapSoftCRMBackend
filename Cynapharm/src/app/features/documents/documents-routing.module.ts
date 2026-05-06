import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'general', pathMatch: 'full' },

  // Documents généraux
  { path: 'general',
    loadComponent: () => import('./documents-general/document-list/document-list.component').then(m => m.DocumentListComponent) },

  // Bons de commande
  { path: 'bons-commandes',
    loadComponent: () => import('./bons-commandes/bon-commande-list/bon-commande-list.component').then(m => m.BonCommandeListComponent) },

  // Bons de livraison
  { path: 'bons-livraison',
    loadComponent: () => import('./bons-livraison/bon-livraison-list/bon-livraison-list.component').then(m => m.BonLivraisonListComponent) },

  // Factures
  { path: 'factures',
    loadComponent: () => import('./factures/facture-list/facture-list.component').then(m => m.FactureListComponent) }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DocumentsRoutingModule {}
