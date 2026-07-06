import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LifecycleService } from '../../core/lifecycle/lifecycle.service';
import { CreateSanctionRequest, Customer, Product, Sanction } from '../../core/lifecycle/lifecycle.models';

@Component({
  selector: 'app-sanctions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './sanctions.component.html'
})
export class SanctionsComponent implements OnInit {
  readonly sanctions = signal<Sanction[]>([]);
  readonly customers = signal<Customer[]>([]);
  readonly products = signal<Product[]>([]);
  readonly showForm = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly repaymentMethods = ['Level Principal', 'Annuity', 'PPMT Principal', 'Scheduled Principal'];
  selectedCustomerId = '';
  form: CreateSanctionRequest = this.blank();

  constructor(private readonly service: LifecycleService) {}

  ngOnInit(): void {
    this.reload();
    this.service.listCustomers().subscribe((c) => this.customers.set(c));
    this.service.listProducts().subscribe((p) => this.products.set(p));
  }

  reload(): void {
    this.service.listSanctions().subscribe({
      next: (s) => this.sanctions.set(s),
      error: () => this.errorMessage.set('Could not load sanctions.')
    });
  }

  onCustomerChange(id: string): void {
    this.selectedCustomerId = id;
    const c = this.customers().find((x) => x.id === id);
    this.form.customerId = id;
    this.form.customerNo = c?.customerNo ?? '';
  }

  onProductChange(code: string): void {
    this.form.productCode = code;
    const p = this.products().find((x) => x.productCode === code);
    if (p) {
      this.form.repaymentMethod = p.repaymentMethod;
      this.form.dayCountBasis = p.dayCountBasis;
      this.form.initialInterestRatePercent = p.suggestedRatePercent;
    }
  }

  save(): void {
    this.saving.set(true);
    this.errorMessage.set(null);
    this.service.createSanction(this.form).subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.form = this.blank();
        this.selectedCustomerId = '';
        this.reload();
      },
      error: (e) => {
        this.saving.set(false);
        this.errorMessage.set(e?.error?.detail ?? e?.error?.title ?? 'Save failed (sanction id may already exist).');
      }
    });
  }

  sign(s: Sanction): void {
    this.service.signSanction(s.id).subscribe({
      next: () => this.reload(),
      error: (e) => this.errorMessage.set(e?.error?.detail ?? 'Sign failed.')
    });
  }

  private blank(): CreateSanctionRequest {
    return {
      sanctionId: '', customerId: '', customerNo: '', productCode: '', projectName: '',
      industryType: null, loanCurrency: 'BDT', loanAmount: 0, grantCurrency: 'BDT', grantAmount: 0,
      agreementDate: new Date().toISOString().slice(0, 10), expiryDate: null,
      interestRateType: 'Fixed', initialInterestRatePercent: 9, loanTenorMonths: 84,
      noOfPrincipalRepayments: 24, interestGracePeriodMonths: 0, principalMoratoriumMonths: 12,
      repaymentMethod: 'Level Principal', principalFrequency: 4, interestFrequency: 4,
      dayCountBasis: 360, lpcRatePercent: 2, creditRating: null
    };
  }
}
