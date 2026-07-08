import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BorrowerExample, ScheduleParameters, ScheduleRow } from '../../core/repayment/repayment.models';
import { RepaymentService } from '../../core/repayment/repayment.service';
import { serialToIso } from '../../core/repayment/serial-date';

@Component({
  selector: 'app-repayment-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './repayment-schedule.component.html',
  styleUrl: './repayment-schedule.component.scss'
})
export class RepaymentScheduleComponent implements OnInit {
  readonly examples = signal<BorrowerExample[]>([]);
  readonly selectedKey = signal<string>('');
  readonly params = signal<ScheduleParameters | null>(null);
  readonly schedule = signal<ScheduleRow[]>([]);
  readonly computing = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // Editable scalar params surfaced as a simple object bound to the form; the array-valued params
  // (disbursements, repayment dates, per-period overrides) come from the loaded example and are
  // sent through unchanged.
  form = {
    projectName: '',
    currency: 'BDT',
    loanAmount: 0,
    interestRatePercent: 0,
    dayCountBasis: 360,
    numInstallments: 0,
    principalType: 'Level Principal',
    paymentFrequency: 4
  };

  readonly principalTypes = ['Level Principal', 'Annuity', 'PPMT Principal', 'Scheduled Principal', 'Scheduled Percentage Principal'];

  readonly totals = computed(() => {
    const rows = this.schedule();
    return {
      interest: rows.reduce((s, r) => s + r.interest, 0),
      principal: rows.reduce((s, r) => s + r.principal, 0),
      tds: rows.reduce((s, r) => s + r.tds, 0)
    };
  });

  constructor(private readonly repaymentService: RepaymentService) {}

  ngOnInit(): void {
    this.repaymentService.loadExamples().subscribe({
      next: (examples) => this.examples.set(examples),
      error: () => this.errorMessage.set('Could not load borrower examples.')
    });
  }

  onSelectExample(key: string): void {
    this.selectedKey.set(key);
    const example = this.examples().find((e) => e.key === key);
    if (!example) return;

    const p = structuredClone(example.parameters);
    this.params.set(p);
    this.form = {
      projectName: p.projectName,
      currency: p.currency,
      loanAmount: p.loanAmount,
      interestRatePercent: +(p.interestRate * 100).toFixed(6),
      dayCountBasis: p.dayCountBasis,
      numInstallments: p.numInstallments,
      principalType: p.principalType,
      paymentFrequency: p.paymentFrequency
    };
    this.generate();
  }

  generate(): void {
    const base = this.params();
    if (!base) {
      this.errorMessage.set('Select a borrower example first.');
      return;
    }

    // Merge the edited scalars back over the example's full parameter set.
    const merged: ScheduleParameters = {
      ...base,
      projectName: this.form.projectName,
      currency: this.form.currency,
      loanAmount: this.form.loanAmount,
      interestRate: this.form.interestRatePercent / 100,
      dayCountBasis: this.form.dayCountBasis,
      numInstallments: this.form.numInstallments,
      principalType: this.form.principalType,
      paymentFrequency: this.form.paymentFrequency
    };

    this.computing.set(true);
    this.errorMessage.set(null);
    this.repaymentService.compute(merged).subscribe({
      next: (rows) => {
        this.schedule.set(rows);
        this.computing.set(false);
      },
      error: () => {
        this.errorMessage.set('Schedule computation failed. Is the API running?');
        this.computing.set(false);
      }
    });
  }

  isoDate(serial: number): string {
    return serialToIso(serial);
  }
}
