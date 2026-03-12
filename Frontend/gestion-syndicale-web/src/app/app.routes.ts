import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { superAdminGuard } from './core/guards/super-admin.guard';
import { MainLayoutComponent } from './shared/layouts/main-layout.component';

export const routes: Routes = [
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES)
  },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'settings',
        loadChildren: () => import('./features/settings/settings.routes').then(m => m.SETTINGS_ROUTES),
        canActivate: [superAdminGuard]
      },
      {
        path: 'parking',
        loadChildren: () => import('./features/parking/parking.routes').then(m => m.PARKING_ROUTES),
        canActivate: [superAdminGuard]
      },
      {
        path: 'finances',
        loadChildren: () => import('./features/finances/finances.routes').then(m => m.FINANCES_ROUTES),
        canActivate: [superAdminGuard]
      },
      {
        path: 'admin/announcements',
        loadComponent: () => import('./features/admin/announcements/admin-announcements-list.component').then(m => m.AdminAnnouncementsListComponent),
        canActivate: [superAdminGuard]
      },
      {
        path: 'admin/suppliers',
        loadComponent: () => import('./features/admin/suppliers/admin-suppliers-list.component').then(m => m.AdminSuppliersListComponent),
        canActivate: [superAdminGuard]
      },
      {
        path: 'admin/polls',
        loadComponent: () => import('./features/admin/polls/admin-polls-list.component').then(m => m.AdminPollsListComponent),
        canActivate: [superAdminGuard]
      },
      {
        path: 'portal/announcements',
        loadComponent: () => import('./features/portal/announcements/portal-announcements.component').then(m => m.PortalAnnouncementsComponent)
      },
      {
        path: 'portal/polls',
        loadComponent: () => import('./features/portal/polls/portal-polls.component').then(m => m.PortalPollsComponent)
      },
      {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full'
      }
    ]
  }
];
