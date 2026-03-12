import { Routes } from '@angular/router';

export const FINANCES_ROUTES: Routes = [
  {
    path: 'revenues',
    loadComponent: () => import('./revenues/revenues.component').then(m => m.RevenuesComponent)
  },
  {
    path: 'expenses',
    loadComponent: () => import('./expenses/expenses.component').then(m => m.ExpensesComponent)
  },
  {
    path: 'reports',
    loadComponent: () => import('./reports/reports.component').then(m => m.ReportsComponent)
  },
  {
    path: '',
    redirectTo: 'revenues',
    pathMatch: 'full'
  }
];
