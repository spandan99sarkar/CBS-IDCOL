import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReportCatalogEntry, ReportResult, ReportsService } from '../../core/reports/reports.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reports.component.html'
})
export class ReportsComponent implements OnInit {
  readonly catalog = signal<ReportCatalogEntry[]>([]);
  readonly selected = signal<ReportCatalogEntry | null>(null);
  readonly report = signal<ReportResult | null>(null);
  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  from = '2026-01-01';
  to = '2026-12-31';

  readonly groups = computed(() => {
    const g: Record<string, ReportCatalogEntry[]> = {};
    for (const c of this.catalog()) (g[c.group] ??= []).push(c);
    return Object.entries(g).map(([group, items]) => ({ group, items }));
  });

  constructor(private readonly service: ReportsService) {}

  ngOnInit(): void {
    this.service.catalog().subscribe({
      next: (c) => {
        this.catalog.set(c);
        if (c.length) this.select(c[0]);
      },
      error: () => this.errorMessage.set('Could not load the report catalog.')
    });
  }

  select(entry: ReportCatalogEntry): void {
    this.selected.set(entry);
    this.run();
  }

  run(): void {
    const entry = this.selected();
    if (!entry) return;
    this.loading.set(true);
    this.errorMessage.set(null);
    this.service.run(entry.key, this.from, this.to).subscribe({
      next: (r) => { this.report.set(r); this.loading.set(false); },
      error: () => { this.errorMessage.set('Failed to run the report.'); this.loading.set(false); }
    });
  }

  cell(row: Record<string, unknown>, key: string): unknown {
    return row[key];
  }

  /** Coerce a report cell/total (typed unknown) to a number for the currency/decimal pipes. */
  num(value: unknown): number {
    return Number(value ?? 0) || 0;
  }

  isNumeric(kind: string): boolean {
    return kind === 'money' || kind === 'int' || kind === 'rate';
  }

  statusClass(value: unknown): string {
    const s = String(value ?? '');
    if (s === 'Standard') return 'ok';
    if (s === 'SMA') return 'warn';
    return 'bad';
  }
}
