import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  { path: '', redirectTo: 'general', pathMatch: 'full' },

  // Documents généraux
  {
    path: 'general',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    loadComponent: () => import('./documents-general/document-list/document-list.component').then(m => m.DocumentListComponent)
  },

  // Bons de commande
  {
    path: 'bons-commandes',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    loadComponent: () => import('./bons-commandes/bon-commande-list/bon-commande-list.component').then(m => m.BonCommandeListComponent)
  },
  {
    path: 'bons-commandes/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'], documentKind: 'bon-commande' },
    loadComponent: () => import('./document-detail/document-detail.component').then(m => m.DocumentDetailComponent)
  },

  // Bons de livraison
  {
    path: 'bons-livraison',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    loadComponent: () => import('./bons-livraison/bon-livraison-list/bon-livraison-list.component').then(m => m.BonLivraisonListComponent)
  },
  {
    path: 'bons-livraison/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'], documentKind: 'bon-livraison' },
    loadComponent: () => import('./document-detail/document-detail.component').then(m => m.DocumentDetailComponent)
  },

  // Factures
  {
    path: 'factures',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'] },
    loadComponent: () => import('./factures/facture-list/facture-list.component').then(m => m.FactureListComponent)
  },
  {
    path: 'factures/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR'], documentKind: 'facture' },
    loadComponent: () => import('./document-detail/document-detail.component').then(m => m.DocumentDetailComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DocumentsRoutingModule { }
