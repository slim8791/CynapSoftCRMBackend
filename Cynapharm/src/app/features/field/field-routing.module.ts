import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'visites', pathMatch: 'full' },

  // Visites
  { path: 'visites',
    loadComponent: () => import('./visites/visite-list/visite-list.component').then(m => m.VisiteListComponent) },
  { path: 'visites/all',
    loadComponent: () => import('./visites/visite-all/visite-all.component').then(m => m.VisiteAllComponent) },
  { path: 'visites/new',
    loadComponent: () => import('./visites/visite-form/visite-form.component').then(m => m.VisiteFormComponent) },
  { path: 'visites/:id/edit',
    loadComponent: () => import('./visites/visite-form/visite-form.component').then(m => m.VisiteFormComponent) },

  // Plannings
  { path: 'plannings',
    loadComponent: () => import('./plannings/planning-list/planning-list.component').then(m => m.PlanningListComponent) },
  { path: 'plannings/new',
    loadComponent: () => import('./plannings/planning-form/planning-form.component').then(m => m.PlanningFormComponent) },
  { path: 'plannings/:id/edit',
    loadComponent: () => import('./plannings/planning-form/planning-form.component').then(m => m.PlanningFormComponent) },

  // Rapports
  { path: 'rapports',
    loadComponent: () => import('./rapports/rapport-list/rapport-list.component').then(m => m.RapportListComponent) },
  { path: 'rapports/new',
    loadComponent: () => import('./rapports/rapport-form/rapport-form.component').then(m => m.RapportFormComponent) },
  { path: 'rapports/:id/edit',
    loadComponent: () => import('./rapports/rapport-form/rapport-form.component').then(m => m.RapportFormComponent) },

  // Objectifs
  { path: 'objectifs',
    loadComponent: () => import('./objectifs/objectif-list/objectif-list.component').then(m => m.ObjectifListComponent) },
  { path: 'objectifs/new',
    loadComponent: () => import('./objectifs/objectif-form/objectif-form.component').then(m => m.ObjectifFormComponent) },
  { path: 'objectifs/:id/edit',
    loadComponent: () => import('./objectifs/objectif-form/objectif-form.component').then(m => m.ObjectifFormComponent) },

  // Régions
  { path: 'regions',
    loadComponent: () => import('./regions/region-list/region-list.component').then(m => m.RegionListComponent) },
  { path: 'regions/new',
    loadComponent: () => import('./regions/region-form/region-form.component').then(m => m.RegionFormComponent) },
  { path: 'regions/:id/edit',
    loadComponent: () => import('./regions/region-form/region-form.component').then(m => m.RegionFormComponent) },

  // KPI
  { path: 'kpi',
    loadComponent: () => import('./kpi/kpi-dashboard/kpi-dashboard.component').then(m => m.KpiDashboardComponent) }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class FieldRoutingModule {}
