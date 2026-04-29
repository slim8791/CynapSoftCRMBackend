import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { ProductService } from '../product.service';
import { CardComponent } from '../../shared/components/card/card.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, CardComponent, ButtonComponent],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.css'
})
export class ProductFormComponent implements OnInit {
  productForm: FormGroup;
  isEditMode: boolean = false;
  loading: boolean = false;
  error: string = '';
  success: boolean = false;
  private productId: string = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required]],
      price: ['', [Validators.required, Validators.min(0)]],
      quantity: ['', [Validators.required, Validators.min(0)]],
      description: ['', []]
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
    this.productService.getProductById(this.productId).subscribe({
      next: (data) => {
        this.productForm.patchValue(data);
      },
      error: (err) => {
        this.error = 'Failed to load product data';
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (!this.productForm.valid) return;

    this.loading = true;
    this.error = '';
    this.success = false;

    const formData = this.productForm.value;

    const request = this.isEditMode
      ? this.productService.updateProduct(this.productId, formData)
      : this.productService.createProduct(formData);

    request.subscribe({
      next: () => {
        this.success = true;
        this.loading = false;
        setTimeout(() => {
          this.router.navigate(['/products']);
        }, 1500);
      },
      error: (err) => {
        this.error = `Failed to ${this.isEditMode ? 'update' : 'create'} product`;
        this.loading = false;
        console.error(err);
      }
    });
  }
}
