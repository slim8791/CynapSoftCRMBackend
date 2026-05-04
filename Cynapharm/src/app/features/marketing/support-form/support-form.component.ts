import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MarketingService } from '../marketing.service';
import { CardComponent } from '../../../shared/components/card/card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-support-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CardComponent,
    ButtonComponent
  ],
  templateUrl: './support-form.component.html',
  styleUrls: ['./support-form.component.css']
})
export class SupportFormComponent implements OnInit {

  supportForm: FormGroup;
  isEditMode = false;
  loading = false;
  error = '';
  success = false;

  productId!: number;
  private supportId!: number;
  supportTypes = ['Brochure', 'Video', 'Image', 'PDF', 'Document', 'Presentation', 'Other'];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private marketingService: MarketingService
  ) {
    this.supportForm = this.fb.group({
      type: ['', Validators.required],
      campaignName: [''],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.productId = Number(this.route.snapshot.queryParamMap.get('productId'));
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEditMode = true;
      this.supportId = Number(idParam);
      this.loadSupport();
    }
  }

  private loadSupport(): void {
    this.loading = true;
    this.marketingService.getSupportById(this.supportId).subscribe({
      next: (response: any) => {
        const data = response.result || response;
        this.supportForm.patchValue({
          type: data.type ?? data.Type,
          campaignName: data.campaignName ?? data.CampaignName,
          isActive: data.isActive ?? data.IsActive ?? true
        });
        this.loading = false;
      },
      error: (err: any) => {
        console.error('❌ Load error:', err);
        this.loading = false;
        this.error = 'Failed to load support';
      }
    });
  }

  onSubmit(): void {
    if (this.supportForm.invalid) {
      Object.keys(this.supportForm.controls).forEach(key => {
        this.supportForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = false;

    const formValue = this.supportForm.value;
    const payload = {
      type: formValue.type,
      idProduit: this.productId,
      isActive: formValue.isActive,
      campaignName: formValue.campaignName
    };

    console.log('📤 Submitting support:', payload);

    this.marketingService.createOrUpdateSupport(payload).subscribe({
      next: (response: any) => {
        console.log('✅ Support saved:', response);
        this.success = true;
        this.loading = false;

        setTimeout(() => {
          this.router.navigate(['/marketing/supports'], {
            queryParams: { productId: this.productId }
          });
        }, 1200);
      },
      error: (err: any) => {
        console.error('❌ Save error:', err);
        this.loading = false;
        this.error = err.error?.message || 'Failed to save support';
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/marketing/supports'], {
      queryParams: { productId: this.productId }
    });
  }
}
