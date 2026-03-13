import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { ConfigService } from '../../core/services/config.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule
  ],
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  user = this.authService.getCurrentUser();
  isSuperAdmin = false;
  residenceName = '';
  isMobile = false;
  sidenavOpened = true;
  private breakpointSub!: Subscription;

  constructor(
    private authService: AuthService,
    private router: Router,
    private configService: ConfigService,
    private breakpointObserver: BreakpointObserver
  ) {}

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.residenceName = this.configService.getResidenceName();

    this.breakpointSub = this.breakpointObserver
      .observe([Breakpoints.Handset, Breakpoints.TabletPortrait])
      .subscribe(result => {
        this.isMobile = result.matches;
        this.sidenavOpened = !this.isMobile;
      });
  }

  ngOnDestroy(): void {
    this.breakpointSub?.unsubscribe();
  }

  onNavItemClick(drawer: any): void {
    if (this.isMobile) {
      drawer.close();
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
