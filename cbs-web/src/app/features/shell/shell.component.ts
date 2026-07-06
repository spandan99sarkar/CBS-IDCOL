import { Component } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavGroup {
  label: string;
  items: { label: string; route: string; enabled: boolean }[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent {
  // Placeholder nav mirroring the department-grouped structure from the validated prototype.
  // Only System Administration is wired up in Phase 0; the rest light up module by module.
  readonly navGroups: NavGroup[] = [
    {
      label: 'System Administration',
      items: [
        { label: 'Dashboard', route: '/', enabled: true },
        { label: 'Audit Trail', route: '/', enabled: true }
      ]
    },
    {
      label: 'Admin',
      items: [{ label: 'Product Configuration', route: '/', enabled: false }]
    },
    {
      label: 'BU / CRM',
      items: [{ label: 'Loan Agreements', route: '/', enabled: false }]
    },
    {
      label: 'CAD',
      items: [
        { label: 'Disbursement Review', route: '/', enabled: false },
        { label: 'Repayment Schedule', route: '/', enabled: false }
      ]
    },
    {
      label: 'Accounts',
      items: [{ label: 'Disbursement Posting', route: '/', enabled: false }]
    }
  ];

  constructor(readonly authService: AuthService, private readonly router: Router) {}

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }
}
