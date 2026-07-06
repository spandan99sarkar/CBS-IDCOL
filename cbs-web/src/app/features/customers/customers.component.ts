import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LifecycleService } from '../../core/lifecycle/lifecycle.service';
import { Customer, CreateCustomerRequest } from '../../core/lifecycle/lifecycle.models';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customers.component.html'
})
export class CustomersComponent implements OnInit {
  readonly customers = signal<Customer[]>([]);
  readonly showForm = signal(false);
  readonly saving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  form: CreateCustomerRequest = this.blank();

  constructor(private readonly service: LifecycleService) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.service.listCustomers().subscribe({
      next: (c) => this.customers.set(c),
      error: () => this.errorMessage.set('Could not load customers.')
    });
  }

  save(): void {
    this.saving.set(true);
    this.errorMessage.set(null);
    this.service.createCustomer(this.form).subscribe({
      next: () => {
        this.saving.set(false);
        this.showForm.set(false);
        this.form = this.blank();
        this.reload();
      },
      error: (e) => {
        this.saving.set(false);
        this.errorMessage.set(e?.error?.detail ?? e?.error?.title ?? 'Save failed (customer number may already exist).');
      }
    });
  }

  private blank(): CreateCustomerRequest {
    return {
      customerNo: '', customerType: 'Institutional', name: '', businessUnitCode: 'IF',
      mobile: null, email: null, sectorCode: null, kycStatus: 'Pending', riskLevel: 'Low'
    };
  }
}
