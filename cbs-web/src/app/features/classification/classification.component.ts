import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ClassificationService } from '../../core/classification/classification.service';
import { ClassificationAccountInput, ClassificationResult } from '../../core/classification/classification.models';
import { DisbursementService } from '../../core/disbursements/disbursement.service';
import { CollectionService } from '../../core/collections/collection.service';
import { LifecycleService } from '../../core/lifecycle/lifecycle.service';

interface Candidate extends ClassificationAccountInput {
  disbursed: number;
  collectedPrincipal: number;
}

@Component({
  selector: 'app-classification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './classification.component.html'
})
export class ClassificationComponent implements OnInit {
  readonly candidates = signal<Candidate[]>([]);
  readonly results = signal<ClassificationResult[]>([]);
  readonly busy = signal(false);
  readonly errorMessage = signal<string | null>(null);
  asOfDate = new Date().toISOString().slice(0, 10);

  readonly canRun = computed(() => (this.auth.currentUser()?.roleCodes ?? []).includes('CAD'));

  readonly totals = computed(() => {
    const r = this.results();
    return {
      outstanding: r.reduce((s, x) => s + x.outstanding, 0),
      provision: r.reduce((s, x) => s + x.provisionRequired, 0),
      classified: r.filter((x) => ['Sub-Standard', 'Doubtful', 'Bad/Loss'].includes(x.status)).length
    };
  });

  constructor(
    private readonly classification: ClassificationService,
    private readonly lifecycle: LifecycleService,
    private readonly disbursements: DisbursementService,
    private readonly collections: CollectionService,
    private readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.reloadResults();
    // Build the candidate worklist: every disbursed loan with its computed outstanding.
    forkJoin({
      sanctions: this.lifecycle.listSanctions(),
      disb: this.disbursements.list(),
      coll: this.collections.list()
    }).subscribe({
      next: ({ sanctions, disb, coll }) => {
        const candidates: Candidate[] = [];
        for (const s of sanctions) {
          const disbForLoan = disb.filter((d) => d.sanctionRef === s.sanctionId && d.status === 'Processed');
          if (disbForLoan.length === 0) continue; // only disbursed loans are classified
          const disbursed = disbForLoan.reduce((sum, d) => sum + d.effectiveLoanAmount, 0);
          const collectedPrincipal = coll
            .filter((c) => c.sanctionRef === s.sanctionId && c.status === 'Verified')
            .reduce((sum, c) => sum + c.principalAmount, 0);
          const outstanding = Math.max(0, disbursed - collectedPrincipal);
          candidates.push({
            accountId: s.id,
            accountRef: s.sanctionId,
            customerNo: s.customerNo,
            projectName: s.projectName,
            currency: s.loanCurrency,
            financeType: s.loanTenorMonths <= 12 ? 'ShortTerm' : 'Term',
            tenorMonths: s.loanTenorMonths,
            isCmsme: false,
            outstanding,
            overdueMonths: 0,
            interestSuspense: 0,
            eligibleCollateral: 0,
            qualitativeOverride: null,
            disbursed,
            collectedPrincipal
          });
        }
        this.candidates.set(candidates);
      },
      error: () => this.errorMessage.set('Could not build the classification worklist.')
    });
  }

  reloadResults(): void {
    this.classification.list().subscribe({
      next: (r) => this.results.set(r),
      error: () => {}
    });
  }

  run(): void {
    if (this.candidates().length === 0) {
      this.errorMessage.set('No disbursed loans to classify. Disburse a loan first.');
      return;
    }
    this.busy.set(true);
    this.errorMessage.set(null);
    const accounts: ClassificationAccountInput[] = this.candidates().map((c) => ({
      accountId: c.accountId, accountRef: c.accountRef, customerNo: c.customerNo, projectName: c.projectName,
      currency: c.currency, financeType: c.financeType, tenorMonths: c.tenorMonths, isCmsme: c.isCmsme,
      outstanding: c.outstanding, overdueMonths: +c.overdueMonths || 0, interestSuspense: +c.interestSuspense || 0,
      eligibleCollateral: +c.eligibleCollateral || 0, qualitativeOverride: c.qualitativeOverride || null
    }));
    this.classification.run({ asOfDate: this.asOfDate, accounts }).subscribe({
      next: () => { this.busy.set(false); this.reloadResults(); },
      error: (e) => { this.busy.set(false); this.errorMessage.set(e?.error?.detail ?? 'Classification run failed (CAD role required).'); }
    });
  }

  badgeClass(status: string): string {
    switch (status) {
      case 'Standard': return 'signed';
      case 'SMA': return 'pending';
      default: return 'classified';
    }
  }
}
