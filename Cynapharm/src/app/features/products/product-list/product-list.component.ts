import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CurrencyTNDPipe } from '../../../shared/pipes/currency-tnd.pipe';
import { Router, RouterLink } from '@angular/router';
import { ProductService } from '../product.service';
import { AuthService } from '../../../core/services/auth.service';
import { TableComponent } from '../../../shared/components/table/table.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { CardComponent } from '../../../shared/components/card/card.component';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyTNDPipe, RouterLink, TableComponent, ButtonComponent, CardComponent],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit {
  products: any[] = [];
  filteredProducts: any[] = [];
  loading: boolean = false;
  error: string = '';
  successMessage: string = '';
  
  // Modal et filtres
  showConfirmModal: boolean = false;
  confirmAction: string = '';
  confirmProductId: number | null = null;
  confirmProductName: string = '';
  
  statusFilter: 'all' | 'active' | 'inactive' | 'archived' = 'all';

  columns = [
    { key: 'Id_Produit', label: 'ID' },
    { key: 'Nom', label: 'Nom' },
    { key: 'Description', label: 'Description' },
    { key: 'Prix_Vente', label: 'Prix de vente' },
    { key: 'Prix_Creation', label: 'Prix de création' },
    { key: 'TVA', label: 'TVA (%)' },
    { key: 'IsActive', label: 'Statut' },
    { key: 'IsArchived', label: 'Archivé' }
  ];

  constructor(
    private productService: ProductService, 
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  filterProducts(): void {
    if (this.statusFilter === 'all') {
      this.filteredProducts = this.products;
    } else if (this.statusFilter === 'active') {
      this.filteredProducts = this.products.filter(p => p.IsActive && !p.IsArchived);
    } else if (this.statusFilter === 'inactive') {
      this.filteredProducts = this.products.filter(p => !p.IsActive && !p.IsArchived);
    } else if (this.statusFilter === 'archived') {
      this.filteredProducts = this.products.filter(p => p.IsArchived);
    }
  }

  private loadProducts(): void {
    this.loading = true;
    this.error = '';
    this.successMessage = '';
    this.products = [];

console.log('[ProductList] Starting API call to /api/products...');

    this.productService.getProducts().subscribe({
      next: (response: any) => {
        console.log('[ProductList] RAW products:', JSON.stringify(response, null, 2));

        let productsData: any[] = [];
        if (Array.isArray(response)) {
          productsData = response;
        } else if (response) {
          productsData = [response];
        }

        console.log('[ProductList] Parsed products:', JSON.stringify(productsData, null, 2));
        if (productsData.length > 0) {
          console.log('[ProductList] First product keys:', Object.keys(productsData[0]));
        }

        this.products = productsData.map((p: any) => this.normalizeProduct(p));
        console.log('[ProductList] Products count:', this.products.length);
        this.filterProducts();
        this.loading = false;
      },
      error: (err: any) => {
        console.error('[ProductList] API Error:', err);
        
        let errorMsg = 'Erreur de connexion';
        if (err.status === 401) {
          errorMsg = 'Session expiree - Veuillez vous reconnecter';
        } else if (err.status === 403) {
          errorMsg = 'Acces refuse - Droits insuffisants';
        } else if (err.status === 0) {
          errorMsg = 'Serveur inaccessible - Verifier la connexion';
        } else if (err.message) {
          errorMsg = err.message;
        }
        this.error = errorMsg;
        this.loading = false;
      }
    });
  }

  onView(id: number): void {
    console.log('View product:', id);
    this.router.navigate(['/products', id]);
  }

onEdit(id: number): void {
    console.log('Editing product ID:', id, typeof id);
    this.router.navigate(['/products', id.toString(), 'edit']);
  }

  onDelete(id: number): void {
    const product = this.products.find(p => p.Id_Produit === id);
    this.confirmProductId = id;
    this.confirmProductName = product?.Nom || `Produit ${id}`;
    this.confirmAction = 'deactivate';
    this.showConfirmModal = true;
  }

  onArchive(id: number): void {
    const product = this.products.find(p => p.Id_Produit === id);
    this.confirmProductId = id;
    this.confirmProductName = product?.Nom || `Produit ${id}`;
    this.confirmAction = 'archive';
    this.showConfirmModal = true;
  }

onActivate(id: number): void {
    console.log('🚀 [DEBUG] onActivate CLICKED! ID:', id);
    const product = this.products.find(p => p.Id_Produit === id);
    this.confirmProductId = id;
    this.confirmProductName = product?.Nom || `Produit ${id}`;
    this.confirmAction = 'activate';
    this.showConfirmModal = true;
console.log('🚀 [DEBUG] Modal should show. Token:', this.authService.getToken() || 'NO TOKEN FUNC');
  }

confirmAction_execute(): void {
    console.log('🚀 [DEBUG] confirmAction_execute CLICKED! Action:', this.confirmAction, 'ID:', this.confirmProductId);
    console.log('🚀 [DEBUG] Token before API:', this.authService.getToken());
    
if (this.confirmProductId === null) return;
    this.loading = true;
    let action$: Observable<any>;

    if (this.confirmAction === 'deactivate') {
      action$ = this.productService.deleteProduct(this.confirmProductId.toString());
    } else if (this.confirmAction === 'archive') {
      action$ = this.productService.archiveProduct(this.confirmProductId.toString());
    } else if (this.confirmAction === 'activate') {
      action$ = this.productService.activateProduct(this.confirmProductId.toString());
    } else {
      return;
    }

    action$.subscribe({
      next: (response: any) => {
        console.log(`${this.confirmAction} response:`, response);
        if (response && response.IsSuccess !== false) {
          const messages = {
            deactivate: 'Produit désactivé avec succès',
            archive: 'Produit archivé avec succès',
            activate: 'Produit activé avec succès'
          };
          this.successMessage = messages[this.confirmAction as keyof typeof messages];
          this.showConfirmModal = false;
          setTimeout(() => this.loadProducts(), 500);
        } else {
          this.error = response?.Message || `Erreur lors de l'opération`;
          this.loading = false;
        }
      },
      error: (err) => {
        console.error(`${this.confirmAction} error:`, err);
        this.error = `Erreur ${err.status}: ${err.message || 'Serveur indisponible'}`;
        this.loading = false;
      }
    });
  }

  cancelAction(): void {
    this.showConfirmModal = false;
    this.confirmProductId = null;
    this.confirmProductName = '';
    this.confirmAction = '';
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('fr-FR', { 
      style: 'currency', 
      currency: 'TND' 
    }).format(price);
  }

  getActionText(): string {
    const texts = {
      deactivate: 'désactiver',
      archive: 'archiver',
      activate: 'activer'
    };
    return texts[this.confirmAction as keyof typeof texts] || '';
  }

  getConfirmMessage(): string {
    const messages = {
      deactivate: `Êtes-vous sûr de vouloir désactiver le produit "${this.confirmProductName}" ?`,
      archive: `Êtes-vous sûr de vouloir archiver le produit "${this.confirmProductName}" ?`,
      activate: `Êtes-vous sûr de vouloir activer le produit "${this.confirmProductName}" ?`
    };
    return messages[this.confirmAction as keyof typeof messages] || '';
  }

  getStatusText(product: any): string {
    if (product.IsArchived) {
      return 'Archivé';
    }
    return product.IsActive ? 'Actif' : 'Inactif';
  }

  getStatusClass(product: any): string {
    if (product.IsArchived) {
      return 'status-archived';
    }
    return product.IsActive ? 'status-active' : 'status-inactive';
  }

/**
   * Normalize product property names from API snake_case to PascalCase
   * API returns: id_Produit, nom, prix_Vente, tva, isActive, isArchived
   * Component expects: Id_Produit, Nom, Prix_Vente, TVA, IsActive, IsArchived
   * NOTE: Spread FIRST to preserve original values, then override with normalized keys
   */
  private normalizeProduct(product: any): any {
    return {
      ...product,
      Id_Produit: product.id_Produit ?? product.Id_Produit,
      Nom: product.nom ?? product.Nom ?? '',
      Description: product.description ?? product.Description ?? '',
      Prix_Vente: product.prix_Vente ?? product.Prix_Vente ?? 0,
      Prix_Creation: product.prix_Creation ?? product.Prix_Creation ?? 0,
      TVA: product.tVA ?? product.tva ?? product.TVA ?? 0,
      IsActive: product.isActive ?? product.IsActive ?? false,
      IsArchived: product.isArchived ?? product.IsArchived ?? false
    };
  }
}
