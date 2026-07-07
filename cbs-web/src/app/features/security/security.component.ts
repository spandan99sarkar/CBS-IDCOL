import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SecurityInstrument, SecuritySummary, SecurityService } from '../../core/security/security.service';

@Component({
  selector: 'app-security',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './security.component.html'
})
export class SecurityComponent implements OnInit {
  readonly instruments = signal<SecurityInstrument[]>([]);
  readonly summary = signal<SecuritySummary | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // Letter preview modal state.
  readonly letterOpenFor = signal<SecurityInstrument | null>(null);
  readonly letterBody = signal<string | null>(null);
  readonly letterRef = signal<string | null>(null);
  readonly letterOptions = signal<{ family: string; letterType: string; purpose: string }[]>([]);
  selectedLetterType = '';

  category = '';
  family = '';
  readonly families = ['BankGuarantee', 'FDR', 'MTDR', 'DSRA', 'LandMortgage', 'InsurancePolicy', 'CreditRating', 'FinancialStatement', 'PDC', 'MonitoringFee'];

  constructor(private readonly service: SecurityService) {}

  ngOnInit(): void {
    this.service.summary().subscribe({ next: (s) => this.summary.set(s), error: () => {} });
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.service.list(this.category || undefined, this.family || undefined).subscribe({
      next: (i) => { this.instruments.set(i); this.loading.set(false); },
      error: () => { this.errorMessage.set('Could not load the security register.'); this.loading.set(false); }
    });
  }

  /** Row urgency class from days-to-expiry (drives the highlight + badge colour). */
  urgency(i: SecurityInstrument): 'expired' | 'soon' | 'watch' | 'ok' {
    if (i.daysLeft === null) return 'ok';
    if (i.daysLeft < 0) return 'expired';
    if (i.daysLeft <= 30) return 'soon';
    if (i.daysLeft <= 90) return 'watch';
    return 'ok';
  }

  actionClass(action: string): string {
    if (action.startsWith('Expired')) return 'bad';
    if (action.startsWith('Send reminder')) return 'bad';
    if (action.startsWith('Send renewal')) return 'warn';
    if (action.startsWith('Follow up')) return 'warn';
    return 'ok';
  }

  openLetters(i: SecurityInstrument): void {
    this.letterOpenFor.set(i);
    this.letterBody.set(null);
    this.letterRef.set(null);
    this.selectedLetterType = '';
    this.service.letters(i.instrumentFamily).subscribe((opts) => {
      this.letterOptions.set(opts);
      if (opts.length) { this.selectedLetterType = opts[0].letterType; }
    });
  }

  generate(): void {
    const i = this.letterOpenFor();
    if (!i || !this.selectedLetterType) return;
    this.service.generateLetter(i.id, this.selectedLetterType).subscribe({
      next: (r) => { this.letterBody.set(r.body); this.letterRef.set(r.refNo); },
      error: () => this.errorMessage.set('Failed to generate the letter.')
    });
  }

  closeLetters(): void {
    this.letterOpenFor.set(null);
  }
}
