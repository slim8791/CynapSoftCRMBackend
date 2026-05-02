import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { ProductService } from '../product.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.css']
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  isEditMode: boolean = false;
  loading: boolean = false;
  error: string = '';
  success: boolean = false;
  private productId: string = '';

  formatDecimal(event: any, controlName: string) {
    let value = event.target.value.replace(',', '.');
    // Only allow digits, dot, and limit to 2 decimals
    value = value.replace(/[^\d.]/g, '').replace(/\.+/g, '.');
    if (value.split('.').length > 2) {
      value = value.replace(/\.+$/, '');
    }
    this.productForm.get(controlName)?.setValue(value, {emitEvent: false});
  }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) {
    // Match fields to backend DTO: ProduitDto
    // { Id_Produit, Nom, Description, Prix_Vente, Prix_Creation, TVA, IsActive, IsArchived, Lots, Supports }
    this.productForm = this.fb.group({
      nom: ['', [Validators.required]],
      description: [''],
      prix_Vente: ['', [Validators.required, Validators.min(0)]],
      prix_Creation: ['', [Validators.required, Validators.min(0)]],
      tVA: [19, [Validators.required, Validators.min(0), Validators.max(100)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.productId = params['id'];
        this.loadProductData();
      }
    });
  }

  private loadProductData(): void {
    console.log('[ProductForm] Loading product ID:', this.productId);
    this.loading = true;
    this.productService.getProductById(this.productId).subscribe({
      next: (data: any) => {
        console.log('[ProductForm] RAW API response:', data);
        console.log('[ProductForm] Unwrapped product:', data.Result || data);
        // Handle both direct DTO and wrapped response
        const product = data.Result || data;
        
        this.productForm.patchValue({
          nom: product.Nom || product.nom || '',
          description: product.Description || product.description || '',
          prix_Vente: product.Prix_Vente || product.prix_Vente || 0,
          prix_Creation: product.Prix_Creation || product.prix_Creation || 0,
          tVA: product.TVA || product.tVA || product.tva || 19,
          isActive: product.IsActive !== false || product.isActive !== false
        });
        console.log('[ProductForm] Form patched with:', this.productForm.value);
        this.loading = false;
      },
      error: (err) => {
        console.error('[ProductForm] API Error:', err);
        this.error = `Erreur lors du chargement du produit ${this.productId}: ${err.status || err.message}`;
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.productForm.valid) return;

    this.loading = true;
    this.error = '';
    this.success = false;

    const formValue = this.productForm.value;
    
    // Match backend DTO: ProduitDto
    const productData = {
      Id_Produit: this.isEditMode ? parseInt(this.productId) : 0,
      Nom: formValue.nom,
      Description: formValue.description || '',
      Prix_Vente: parseFloat(formValue.prix_Vente.toString().replace(',', '.')),
      Prix_Creation: parseFloat(formValue.prix_Creation.toString().replace(',', '.')),
      TVA: parseInt(formValue.tVA),
      IsActive: formValue.isActive !== false,
      IsArchived: false
    };

    // Backend uses POST for both create and update
    const request = this.productService.createProduct(productData);

    request.subscribe({
      next: (response: any) => {
        console.log('[ProductForm] Response:', response);
        this.success = true;
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/products']);
        }, 1500);
      },
      error: (err) => {
        console.error('[ProductForm] Error:', err);
        this.error = `Erreur lors de ${this.isEditMode ? 'la mise à jour' : 'la création'} du produit`;
        this.loading = false;
      }
    });
  }
}
