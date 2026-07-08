import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { DisbursementService } from '../../core/disbursements/disbursement.service';
import { Disbursement, InitiateDisbursementRequest } from '../../core/disbursements/disbursement.models';
import { LifecycleService } from '../../core/lifecycle/lifecycle.service';
import { Sanction } from '../../core/lifecycle/lifecycle.models';

@Component({
  selector: 'app-disbursements',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './disbursements.component.html'
})
export class DisbursementsComponent implements OnInit {
  readonly disbursements = signal<Disbursement[]>([]);
  readonly signedSanctions = signal<Sanction[]>([]);
  readonly showInitiate = signal(false);
  readonly busy = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // Which row (by id) has its review / post panel open.
  readonly openReview = signal<string | null>(null);
  readonly openPost = signal<string | null>(null);
  // Rows whose GL posting detail is expanded (collapsed by default for readability).
  readonly expandedGl = signal<ReadonlySet<string>>(new Set());

  toggleGl(id: string): void {
    const next = new Set(this.expandedGl());
    next.has(id) ? next.delete(id) : next.add(id);
    this.expandedGl.set(next);
  }

  readonly roles = computed(() => this.auth.currentUser()?.roleCodes ?? []);
  readonly canInitiate = computed(() => this.roles().includes('BU'));
  readonly canReview = computed(() => this.roles().includes('CAD'));
  readonly canPost = computed(() => this.roles().includes('ACCOUNTS'));

  selectedSanctionId = '';
  initiateForm: InitiateDisbursementRequest = this.blankInitiate();
  reviewForm = { justifiedLoanAmount: 0, justifiedGrantAmount: 0, cadRemarks: '' };
  postForm = { disbursementMode: 'EFT', valueDate: new Date().toISOString().slice(0, 10) };
  readonly modes = ['Cheque', 'EFT', 'RTGS', 'PayOrder', 'SWIFT'];

  constructor(
    private readonly service: DisbursementService,
    private readonly lifecycle: LifecycleService,
    private readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.reload();
    this.lifecycle.listSanctions().subscribe((s) =>
      this.signedSanctions.set(s.filter((x) => x.status === 'Signed' || x.status === 'Active')));
  }

  reload(): void {
    this.service.list().subscribe({
      next: (d) => this.disbursements.set(d),
      error: () => this.errorMessage.set('Could not load disbursements.')
    });
  }

  onSanctionChange(id: string): void {
    this.selectedSanctionId = id;
    const s = this.signedSanctions().find((x) => x.id === id);
    if (s) {
      this.initiateForm.sanctionId = s.id;
      this.initiateForm.sanctionRef = s.sanctionId;
      this.initiateForm.customerNo = s.customerNo;
      this.initiateForm.projectName = s.projectName;
      this.initiateForm.loanCurrency = s.loanCurrency;
    }
  }

  initiate(): void {
    this.run(() => this.service.initiate(this.initiateForm), () => {
      this.showInitiate.set(false);
      this.initiateForm = this.blankInitiate();
      this.selectedSanctionId = '';
    });
  }

  startReview(d: Disbursement): void {
    this.openPost.set(null);
    this.openReview.set(d.id);
    this.reviewForm = {
      justifiedLoanAmount: d.suggestedLoanAmount,
      justifiedGrantAmount: d.suggestedGrantAmount,
      cadRemarks: ''
    };
  }

  submitReview(d: Disbursement): void {
    this.run(() => this.service.review(d.id, {
      justifiedLoanAmount: this.reviewForm.justifiedLoanAmount,
      justifiedGrantAmount: this.reviewForm.justifiedGrantAmount,
      cadRemarks: this.reviewForm.cadRemarks || null
    }), () => this.openReview.set(null));
  }

  startPost(d: Disbursement): void {
    this.openReview.set(null);
    this.openPost.set(d.id);
    this.postForm = { disbursementMode: 'EFT', valueDate: new Date().toISOString().slice(0, 10) };
  }

  submitPost(d: Disbursement): void {
    // The system proposes the balanced GL posting: Dr Loan Account / Cr Bank for the effective amount.
    const amount = d.effectiveLoanAmount;
    this.run(() => this.service.post(d.id, {
      disbursementMode: this.postForm.disbursementMode,
      valueDate: this.postForm.valueDate,
      glLines: [
        { glCode: '102030', description: 'Loan Account', debit: amount, credit: 0 },
        { glCode: '202030', description: 'Bank Account', debit: 0, credit: amount }
      ]
    }), () => this.openPost.set(null));
  }

  private run(action: () => any, onSuccess: () => void): void {
    this.busy.set(true);
    this.errorMessage.set(null);
    action().subscribe({
      next: () => { this.busy.set(false); onSuccess(); this.reload(); },
      error: (e: any) => {
        this.busy.set(false);
        this.errorMessage.set(e?.error?.detail ?? e?.error?.title ?? 'Action failed (check role / workflow stage).');
      }
    });
  }

  private blankInitiate(): InitiateDisbursementRequest {
    return {
      sanctionId: '', sanctionRef: '', customerNo: '', projectName: '', loanCurrency: 'BDT',
      suggestedLoanAmount: 0, suggestedGrantAmount: 0, buRemarks: null
    };
  }
}
