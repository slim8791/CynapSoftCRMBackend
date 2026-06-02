import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DistributionService, EchantillonDto } from '../services/distribution.service';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state.component';
import { UserService } from '../../../users/user.service';
import { StockService } from '../../stocks/services/stock.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-distribution-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './distribution-detail.component.html',
  styleUrls: ['./distribution-detail.component.css']
})
export class DistributionDetailComponent implements OnInit, OnDestroy {
  item: EchantillonDto | null = null;
  loading = false;
  error = '';
  delegueName = '';
  medecinName = '';
  pharmacienName = '';
  stockLabel = '';

  private destroy$ = new Subject<void>();

  constructor(
    private route:    ActivatedRoute,
    private router:   Router,
    private svc:      DistributionService,
    private userSvc:  UserService,
    private stockSvc: StockService,
    private toast:    ToastService,
    private cdr:      ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) { this.error = 'Identifiant invalide.'; return; }
    this.loading = true;
    this.svc.getById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: data => {
        this.item = data;
        this.loading = false;
        this.loadDisplayData(data);
        this.cdr.markForCheck();
      },
      error: () => { this.error = 'Impossible de charger la distribution.'; this.loading = false; this.cdr.markForCheck(); }
    });
  }

  private loadDisplayData(item: EchantillonDto): void {
    this.loadUserName(item.id_Delegue, value => this.delegueName = value, 'Délégué');
    if (item.id_Medecin) this.loadUserName(item.id_Medecin, value => this.medecinName = value, 'Médecin');
    else this.medecinName = '—';
    if (item.id_Pharmacien) this.loadUserName(item.id_Pharmacien, value => this.pharmacienName = value, 'Pharmacien');
    else this.pharmacienName = '—';

    this.stockSvc.getById(item.id_Stock).pipe(takeUntil(this.destroy$)).subscribe({
      next: stock => {
        this.stockLabel = `${stock.numeroLot || item.numeroLot} (Qté: ${stock.qteDisponible ?? '—'})`;
        this.cdr.markForCheck();
      },
      error: () => {
        this.stockLabel = `${item.numeroLot || 'Stock'} (Qté: —)`;
        this.cdr.markForCheck();
      }
    });
  }

  private loadUserName(id: number, assign: (value: string) => void, fallbackLabel: string): void {
    this.userSvc.getUserById(id).pipe(takeUntil(this.destroy$)).subscribe({
      next: (res: any) => {
        const u = res?.Result ?? res?.result ?? res;
        assign(u?.fullName ?? u?.FullName ?? u?.name ?? u?.Name ?? u?.email ?? u?.Email ?? `${fallbackLabel} #${id}`);
        this.cdr.markForCheck();
      },
      error: () => {
        assign(`${fallbackLabel} #${id}`);
        this.cdr.markForCheck();
      }
    });
  }

  onDelete(): void {
    if (!this.item?.id_Distribution) return;
    if (!confirm('Supprimer cette distribution ?')) return;
    this.svc.delete(this.item.id_Distribution).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.toast.showSuccess('Distribution supprimée.');
        this.router.navigate(['/inventory/distributions']);
      },
      error: () => this.toast.showError('Erreur lors de la suppression.')
    });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
