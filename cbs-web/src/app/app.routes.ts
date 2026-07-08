import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./features/shell/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent)
      },
      {
        path: 'repayment-schedule',
        loadComponent: () =>
          import('./features/repayment-schedule/repayment-schedule.component').then((m) => m.RepaymentScheduleComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customers.component').then((m) => m.CustomersComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/products/products.component').then((m) => m.ProductsComponent)
      },
      {
        path: 'sanctions',
        loadComponent: () => import('./features/sanctions/sanctions.component').then((m) => m.SanctionsComponent)
      },
      {
        path: 'sanctions/:sanctionId/facility',
        loadComponent: () =>
          import('./features/facility-versions/facility-versions.component').then((m) => m.FacilityVersionsComponent)
      },
      {
        path: 'disbursements',
        loadComponent: () => import('./features/disbursements/disbursements.component').then((m) => m.DisbursementsComponent)
      },
      {
        path: 'collections',
        loadComponent: () => import('./features/collections/collections.component').then((m) => m.CollectionsComponent)
      },
      {
        path: 'classification',
        loadComponent: () => import('./features/classification/classification.component').then((m) => m.ClassificationComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports.component').then((m) => m.ReportsComponent)
      },
      {
        path: 'security',
        loadComponent: () => import('./features/security/security.component').then((m) => m.SecurityComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
