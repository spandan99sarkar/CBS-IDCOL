import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LifecycleService } from '../../core/lifecycle/lifecycle.service';
import { CreateProductRequest, Product } from '../../core/lifecycle/lifecycle.models';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products.component.html'
})
export class ProductsComponent implements OnInit {
  readonly products = signal<Product[]>([]);
  readonly showForm = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly repaymentMethods = ['Level Principal', 'Annuity', 'PPMT Principal', 'Scheduled Principal'];
  form: CreateProductRequest = this.blank();

  constructor(private readonly service: LifecycleService) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.service.listProducts().subscribe({
      next: (p) => this.products.set(p),
      error: () => this.errorMessage.set('Could not load products.')
    });
  }

  save(): void {
    this.saving.set(true);
    this.errorMessage.set(null);
    this.service.createProduct(this.form).subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.form = this.blank();
        this.reload();
      },
      error: (e) => {
        this.saving.set(false);
        this.errorMessage.set(e?.error?.detail ?? e?.error?.title ?? 'Save failed (product code may already exist).');
      }
    });
  }

  private blank(): CreateProductRequest {
    return {
      productCode: '', productName: '', productType: 'Term Loan', interestType: 'Fixed',
      repaymentMethod: 'Level Principal', dayCountBasis: 360, gracePeriodMonths: 0,
      prepaymentAllowed: true, penaltyAllowed: true, suggestedRatePercent: 9,
      lowerRatePercent: 6, upperRatePercent: 15
    };
  }
}
