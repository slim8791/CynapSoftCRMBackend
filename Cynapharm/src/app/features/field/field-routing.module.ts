import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

const routes: Routes = [
  { path: '', redirectTo: 'visites', pathMatch: 'full' },

  // Visites
  {
    path: 'visites',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./visites/visite-list/visite-list.component').then(m => m.VisiteListComponent)
  },
  {
    path: 'visites/all',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./visites/visite-all/visite-all.component').then(m => m.VisiteAllComponent)
  },
  {
    path: 'visites/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./visites/visite-form/visite-form.component').then(m => m.VisiteFormComponent)
  },
  {
    path: 'visites/:id',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./visites/visite-detail/visite-detail.component').then(m => m.VisiteDetailComponent)
  },
  {
    path: 'visites/:id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./visites/visite-form/visite-form.component').then(m => m.VisiteFormComponent)
  },

  // Plannings
  {
    path: 'plannings',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./plannings/planning-list/planning-list.component').then(m => m.PlanningListComponent)
  },
  {
    path: 'plannings/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./plannings/planning-form/planning-form.component').then(m => m.PlanningFormComponent)
  },
  {
    path: 'plannings/:id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./plannings/planning-form/planning-form.component').then(m => m.PlanningFormComponent)
  },

  // Rapports
  {
    path: 'rapports',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./rapports/rapport-list/rapport-list.component').then(m => m.RapportListComponent)
  },
  {
    path: 'rapports/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'DELEGUE'] },
    loadComponent: () => import('./rapports/rapport-form/rapport-form.component').then(m => m.RapportFormComponent)
  },
  {
    path: 'rapports/:id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'DELEGUE'] },
    loadComponent: () => import('./rapports/rapport-form/rapport-form.component').then(m => m.RapportFormComponent)
  },

  // Objectifs
  {
    path: 'objectifs',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./objectifs/objectif-list/objectif-list.component').then(m => m.ObjectifListComponent)
  },
  {
    path: 'objectifs/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./objectifs/objectif-form/objectif-form.component').then(m => m.ObjectifFormComponent)
  },
  {
    path: 'objectifs/:id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./objectifs/objectif-form/objectif-form.component').then(m => m.ObjectifFormComponent)
  },

  // Régions
  {
    path: 'regions',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./regions/region-list/region-list.component').then(m => m.RegionListComponent)
  },
  {
    path: 'regions/new',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./regions/region-form/region-form.component').then(m => m.RegionFormComponent)
  },
  {
    path: 'regions/:id/edit',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./regions/region-form/region-form.component').then(m => m.RegionFormComponent)
  },

  // KPI
  {
    path: 'kpi',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN', 'SUPERVISEUR', 'DELEGUE'] },
    loadComponent: () => import('./kpi/kpi-dashboard/kpi-dashboard.component').then(m => m.KpiDashboardComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FieldRoutingModule { }
