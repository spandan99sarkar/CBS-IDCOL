import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { AuditLogEntry } from '../../core/audit/audit.models';
import { AuditService } from '../../core/audit/audit.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  readonly entries = signal<AuditLogEntry[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor(private readonly auditService: AuditService) {}

  ngOnInit(): void {
    this.auditService.getRecent().subscribe({
      next: (entries) => {
        this.entries.set(entries);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load the audit trail (is the API reachable and is a database configured?).');
        this.loading.set(false);
      }
    });
  }
}
