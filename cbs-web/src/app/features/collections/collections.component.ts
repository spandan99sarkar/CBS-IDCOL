import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth/auth.service';
import { CollectionService } from '../../core/collections/collection.service';
import { EnterReceiptRequest, Receipt } from '../../core/collections/collection.models';
import { LifecycleService } from '../../core/lifecycle/lifecycle.service';
import { Sanction } from '../../core/lifecycle/lifecycle.models';

@Component({
  selector: 'app-collections',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './collections.component.html'
})
export class CollectionsComponent implements OnInit {
  readonly receipts = signal<Receipt[]>([]);
  readonly sanctions = signal<Sanction[]>([]);
  readonly showForm = signal(false);
  readonly busy = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly roles = computed(() => this.auth.currentUser()?.roleCodes ?? []);
  readonly canEnter = computed(() => this.roles().includes('CAD'));
  readonly canVerify = computed(() => this.roles().includes('ACCOUNTS'));
  // Rows whose GL posting detail is expanded (collapsed by default for readability).
  readonly expandedGl = signal<ReadonlySet<string>>(new Set());

  toggleGl(id: string): void {
    const next = new Set(this.expandedGl());
    next.has(id) ? next.delete(id) : next.add(id);
    this.expandedGl.set(next);
  }

  readonly modes = ['Cash', 'Cheque', 'PayOrder', 'EFT', 'RTGS', 'SWIFT', 'PDC'];
  selectedSanctionId = '';
  form: EnterReceiptRequest = this.blank();

  constructor(
    private readonly service: CollectionService,
    private readonly lifecycle: LifecycleService,
    private readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.reload();
    this.lifecycle.listSanctions().subscribe((s) =>
      this.sanctions.set(s.filter((x) => x.status === 'Signed' || x.status === 'Active')));
  }

  reload(): void {
    this.service.list().subscribe({
      next: (r) => this.receipts.set(r),
      error: () => this.errorMessage.set('Could not load collections.')
    });
  }

  get allocationSum(): number {
    return (+this.form.principalAmount || 0) + (+this.form.interestAmount || 0) + (+this.form.lpcAmount || 0);
  }

  get allocationBalanced(): boolean {
    return Math.round((this.allocationSum - (+this.form.instrumentAmount || 0)) * 100) === 0
      && (+this.form.instrumentAmount || 0) > 0;
  }

  onSanctionChange(id: string): void {
    this.selectedSanctionId = id;
    const s = this.sanctions().find((x) => x.id === id);
    if (s) {
      this.form.sanctionId = s.id;
      this.form.sanctionRef = s.sanctionId;
      this.form.customerNo = s.customerNo;
      this.form.projectName = s.projectName;
      this.form.currency = s.loanCurrency;
    }
  }

  enter(): void {
    this.busy.set(true);
    this.errorMessage.set(null);
    this.service.enter(this.form).subscribe({
      next: () => {
        this.busy.set(false);
        this.showForm.set(false);
        this.form = this.blank();
        this.selectedSanctionId = '';
        this.reload();
      },
      error: (e) => {
        this.busy.set(false);
        this.errorMessage.set(e?.error?.detail ?? e?.error?.title ?? 'Save failed (check role / allocation total).');
      }
    });
  }

  verify(r: Receipt): void {
    this.service.verify(r.id, 'Reconciled with bank statement').subscribe({
      next: () => this.reload(),
      error: (e) => this.errorMessage.set(e?.error?.detail ?? 'Verify failed (check role / stage).')
    });
  }

  private blank(): EnterReceiptRequest {
    const today = new Date().toISOString().slice(0, 10);
    return {
      sanctionId: '', sanctionRef: '', customerNo: '', projectName: '', currency: 'BDT',
      paymentMode: 'EFT', instrumentNo: null, bankName: null, instrumentAmount: 0,
      valueDate: today, receiveDate: today, lpcDate: null,
      principalAmount: 0, interestAmount: 0, lpcAmount: 0
    };
  }
}
