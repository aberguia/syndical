import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { DashboardService } from '../../core/services/dashboard.service';
import { AdminDashboard, AdherentDashboard } from '../../core/models/community.models';
import { AuthService } from '../../core/services/auth.service';
import { BuildingRankingWidgetComponent } from '../../shared/components/building-ranking-widget.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, BuildingRankingWidgetComponent, TranslateModule],
  template: `
    <div class="dashboard-container">
      <div *ngIf="loading" class="loading"><mat-spinner></mat-spinner></div>
      
      <!-- Admin Dashboard -->
      <div *ngIf="!loading && isAdmin && adminData" class="admin-dashboard">
        <h2 class="dashboard-title">
          <mat-icon>dashboard</mat-icon>
          {{ 'DASHBOARD.TITLE' | translate }}
        </h2>
        
        <div class="kpis-grid">
          <mat-card class="kpi-card buildings">
            <mat-card-content>
              <div class="kpi-icon-container">
                <mat-icon class="kpi-icon">apartment</mat-icon>
              </div>
              <div class="kpi-content">
                <div class="kpi-value">{{adminData.kpis.buildingsCount}}</div>
                <div class="kpi-label">{{ 'DASHBOARD.BUILDINGS_COUNT' | translate }}</div>
              </div>
            </mat-card-content>
          </mat-card>
          
          <mat-card class="kpi-card apartments">
            <mat-card-content>
              <div class="kpi-icon-container">
                <mat-icon class="kpi-icon">home</mat-icon>
              </div>
              <div class="kpi-content">
                <div class="kpi-value">{{adminData.kpis.apartmentsCount}}</div>
                <div class="kpi-label">{{ 'DASHBOARD.APARTMENTS_COUNT' | translate }}</div>
              </div>
            </mat-card-content>
          </mat-card>
          
          <mat-card class="kpi-card members">
            <mat-card-content>
              <div class="kpi-icon-container">
                <mat-icon class="kpi-icon">people</mat-icon>
              </div>
              <div class="kpi-content">
                <div class="kpi-value">{{adminData.kpis.adherentsCount}}</div>
                <div class="kpi-label">{{ 'DASHBOARD.MEMBERS_COUNT' | translate }}</div>
              </div>
            </mat-card-content>
          </mat-card>
          
          <mat-card class="kpi-card revenues">
            <mat-card-content>
              <div class="kpi-icon-container">
                <mat-icon class="kpi-icon">trending_up</mat-icon>
              </div>
              <div class="kpi-content">
                <div class="kpi-value">{{formatAmountShort(adminData.kpis.totalRevenuesCurrentYear)}}</div>
                <div class="kpi-label">{{ 'DASHBOARD.REVENUES_YEAR' | translate:{year: currentYear} }}</div>
              </div>
            </mat-card-content>
          </mat-card>
          
          <mat-card class="kpi-card expenses">
            <mat-card-content>
              <div class="kpi-icon-container">
                <mat-icon class="kpi-icon">receipt</mat-icon>
              </div>
              <div class="kpi-content">
                <div class="kpi-value">{{formatAmountShort(adminData.kpis.totalExpensesCurrentYear)}}</div>
                <div class="kpi-label">{{ 'DASHBOARD.EXPENSES_YEAR' | translate:{year: currentYear} }}</div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <div class="sections-grid">
          <mat-card class="section-card announcements">
            <mat-card-header>
              <div mat-card-avatar class="section-avatar announcements-avatar">
                <mat-icon>campaign</mat-icon>
              </div>
              <mat-card-title>{{ 'DASHBOARD.RECENT_ANNOUNCEMENTS' | translate }}</mat-card-title>
              <mat-card-subtitle>{{ 'DASHBOARD.LAST_PUBLICATIONS' | translate }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div *ngIf="adminData.recentAnnouncements.length === 0" class="empty-state">
                <mat-icon>info</mat-icon>
                <p>{{ 'DASHBOARD.NO_RECENT_ANNOUNCEMENTS' | translate }}</p>
              </div>
              <div *ngFor="let a of adminData.recentAnnouncements" class="list-item">
                <mat-icon class="item-icon">article</mat-icon>
                <div class="item-content">
                  <span class="item-title">{{a.title}}</span>
                  <span class="item-meta">
                    <span class="status-badge" [class]="'status-' + a.status.toLowerCase()">{{a.status}}</span>
                    <span class="item-date">{{a.createdOn | date:'short'}}</span>
                  </span>
                </div>
              </div>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary" routerLink="/admin/announcements">
                <mat-icon>arrow_forward</mat-icon>
                {{ 'COMMON.SEE_ALL' | translate }}
              </button>
            </mat-card-actions>
          </mat-card>
          
          <mat-card class="section-card polls">
            <mat-card-header>
              <div mat-card-avatar class="section-avatar polls-avatar">
                <mat-icon>poll</mat-icon>
              </div>
              <mat-card-title>{{ 'DASHBOARD.RECENT_POLLS' | translate }}</mat-card-title>
              <mat-card-subtitle>{{ 'DASHBOARD.ONGOING_VOTES' | translate }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <div *ngIf="adminData.recentPolls.length === 0" class="empty-state">
                <mat-icon>info</mat-icon>
                <p>{{ 'DASHBOARD.NO_RECENT_POLLS' | translate }}</p>
              </div>
              <div *ngFor="let p of adminData.recentPolls" class="list-item">
                <mat-icon class="item-icon">how_to_vote</mat-icon>
                <div class="item-content">
                  <span class="item-title">{{p.question}}</span>
                  <span class="item-meta">
                    <span class="votes-badge">
                      <mat-icon>person</mat-icon>
                      {{p.totalVotes}} votes
                    </span>
                    <span class="item-date">{{p.createdOn | date:'short'}}</span>
                  </span>
                </div>
              </div>
            </mat-card-content>
            <mat-card-actions>
              <button mat-button color="primary" routerLink="/admin/polls">
                <mat-icon>arrow_forward</mat-icon>
                {{ 'COMMON.SEE_ALL' | translate }}
              </button>
            </mat-card-actions>
          </mat-card>
        </div>

        <mat-card class="parking-card-admin">
          <mat-card-header>
            <div mat-card-avatar class="section-avatar parking-avatar">
              <mat-icon>local_parking</mat-icon>
            </div>
            <mat-card-title>{{ 'DASHBOARD.PARKING_LIVE' | translate }}</mat-card-title>
            <mat-card-subtitle>{{ 'DASHBOARD.REALTIME' | translate }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="parking-content-admin">
              <div class="parking-visual">
                <div class="parking-circle">
                  <svg viewBox="0 0 100 100">
                    <circle cx="50" cy="50" r="45" fill="none" stroke="#e0e0e0" stroke-width="10"/>
                    <circle cx="50" cy="50" r="45" fill="none" stroke="#4caf50" stroke-width="10"
                            [attr.stroke-dasharray]="283" 
                            [attr.stroke-dashoffset]="283 - (283 * adminData.parkingLive.availablePlaces / adminData.parkingLive.totalPlaces)"
                            transform="rotate(-90 50 50)"/>
                  </svg>
                  <div class="parking-number">
                    <div class="number">{{adminData.parkingLive.availablePlaces}}</div>
                    <div class="label">{{ 'DASHBOARD.AVAILABLE' | translate }}</div>
                  </div>
                </div>
              </div>
              <div class="parking-details">
                <div class="parking-stat">
                  <mat-icon>directions_car</mat-icon>
                  <div class="stat-info">
                    <div class="stat-value">{{adminData.parkingLive.occupiedPlaces}}</div>
                    <div class="stat-label">{{ 'DASHBOARD.OCCUPIED_PLACES' | translate }}</div>
                  </div>
                </div>
                <div class="parking-stat">
                  <mat-icon>local_parking</mat-icon>
                  <div class="stat-info">
                    <div class="stat-value">{{adminData.parkingLive.totalPlaces}}</div>
                    <div class="stat-label">{{ 'DASHBOARD.TOTAL_PLACES' | translate }}</div>
                  </div>
                </div>
                <div class="parking-stat">
                  <mat-icon>insights</mat-icon>
                  <div class="stat-info">
                    <div class="stat-value">{{getOccupancyRate(adminData.parkingLive)}}%</div>
                    <div class="stat-label">{{ 'DASHBOARD.OCCUPANCY_RATE' | translate }}</div>
                  </div>
                </div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <!-- Adherent Dashboard -->
      <div *ngIf="!loading && !isAdmin && adherentData" class="adherent-dashboard">
        <h2>{{ 'DASHBOARD.MY_TITLE' | translate }}</h2>
        
        <mat-card class="message-card" [ngClass]="adherentData.contribution.isUpToDate ? 'success' : 'warning'">
          <mat-card-content><h3>{{adherentData.message}}</h3></mat-card-content>
        </mat-card>

        <div class="top-cards-grid">
          <mat-card class="contribution-card">
            <mat-card-header><mat-card-title>{{ 'DASHBOARD.MY_CONTRIBUTIONS' | translate }}</mat-card-title></mat-card-header>
            <mat-card-content>
              <p>Appartement {{adherentData.contribution.apartmentNumber}} - Immeuble {{adherentData.contribution.buildingNumber}}</p>
              <div class="contrib-row"><span>{{ 'DASHBOARD.ANNUAL_AMOUNT' | translate }}</span><span>{{formatAmount(adherentData.contribution.annualAmount)}}</span></div>
              <div class="contrib-row"><span>{{ 'DASHBOARD.PAID' | translate }}</span><span class="paid">{{formatAmount(adherentData.contribution.paidAmount)}}</span></div>
              <div class="contrib-row"><span>{{ 'DASHBOARD.REMAINING' | translate }}</span><span [class.warning]="!adherentData.contribution.isUpToDate">{{formatAmount(adherentData.contribution.remainingAmount)}}</span></div>
              <div class="status-badge" [ngClass]="adherentData.contribution.isUpToDate ? 'uptodate' : 'late'">{{adherentData.contribution.status}}</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="parking-card">
            <mat-card-header><mat-card-title>{{ 'DASHBOARD.PARKING_LIVE' | translate }}</mat-card-title></mat-card-header>
            <mat-card-content>
              <div class="parking-content">
                <mat-icon class="parking-icon">local_parking</mat-icon>
                <div class="parking-info">
                  <div class="parking-stats">{{adherentData.parkingLive.availablePlaces}}</div>
                  <div class="parking-label">{{ 'DASHBOARD.AVAILABLE_PLACES' | translate }}</div>
                  <div class="parking-total">{{ 'DASHBOARD.OUT_OF' | translate:{total: adherentData.parkingLive.totalPlaces} }}</div>
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- New Building Ranking Widget -->
        <app-building-ranking-widget></app-building-ranking-widget>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container { padding: 24px; background: #f5f5f5; min-height: 100vh; }
    .loading { display: flex; justify-content: center; padding: 60px; }
    
    /* ============ ADMIN DASHBOARD ============ */
    .admin-dashboard { max-width: 1400px; margin: 0 auto; }
    
    .dashboard-title {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 32px;
      font-weight: 600;
      color: #1976d2;
      margin-bottom: 32px;
    }
    .dashboard-title mat-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
    }
    
    /* KPI Cards */
    .kpis-grid { 
      display: grid; 
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); 
      gap: 20px; 
      margin-bottom: 32px; 
    }
    
    .kpi-card {
      position: relative;
      overflow: hidden;
      transition: all 0.3s ease;
      cursor: pointer;
    }
    .kpi-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0,0,0,0.12);
    }
    .kpi-card mat-card-content {
      display: flex;
      align-items: center;
      gap: 20px;
      padding: 24px !important;
    }
    
    .kpi-icon-container {
      width: 64px;
      height: 64px;
      border-radius: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    .kpi-icon {
      font-size: 36px;
      width: 36px;
      height: 36px;
      color: white;
    }
    
    .kpi-card.buildings .kpi-icon-container {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    .kpi-card.apartments .kpi-icon-container {
      background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
    }
    .kpi-card.members .kpi-icon-container {
      background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
    }
    .kpi-card.revenues .kpi-icon-container {
      background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
    }
    .kpi-card.expenses .kpi-icon-container {
      background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);
    }
    
    .kpi-content {
      flex: 1;
    }
    .kpi-value { 
      font-size: 32px; 
      font-weight: 700; 
      color: rgba(0,0,0,0.87);
      line-height: 1.2;
    }
    .kpi-label { 
      color: rgba(0,0,0,0.6); 
      font-size: 14px;
      margin-top: 4px;
    }
    
    /* Sections Grid */
    .sections-grid { 
      display: grid; 
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); 
      gap: 20px; 
      margin-bottom: 24px; 
    }
    
    .section-card {
      transition: all 0.3s ease;
    }
    .section-card:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(0,0,0,0.1);
    }
    
    .section-avatar {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .section-avatar mat-icon {
      color: white;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }
    .announcements-avatar {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }
    .polls-avatar {
      background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
    }
    .parking-avatar {
      background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
    }
    
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 40px 20px;
      color: rgba(0,0,0,0.4);
    }
    .empty-state mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 12px;
    }
    
    .list-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px;
      border-radius: 8px;
      margin-bottom: 8px;
      transition: background 0.2s ease;
    }
    .list-item:hover {
      background: rgba(0,0,0,0.03);
    }
    .item-icon {
      color: rgba(0,0,0,0.4);
      font-size: 24px;
      width: 24px;
      height: 24px;
      flex-shrink: 0;
      margin-top: 2px;
    }
    .item-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 6px;
    }
    .item-title {
      font-weight: 500;
      color: rgba(0,0,0,0.87);
      line-height: 1.4;
    }
    .item-meta {
      display: flex;
      align-items: center;
      gap: 12px;
      font-size: 12px;
      color: rgba(0,0,0,0.6);
    }
    
    .status-badge {
      padding: 3px 8px;
      border-radius: 12px;
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
    }
    .status-badge.status-draft {
      background: #e0e0e0;
      color: #616161;
    }
    .status-badge.status-published {
      background: #c8e6c9;
      color: #2e7d32;
    }
    .status-badge.status-archived {
      background: #ffecb3;
      color: #f57c00;
    }
    
    .votes-badge {
      display: flex;
      align-items: center;
      gap: 4px;
      padding: 3px 8px;
      background: #e3f2fd;
      border-radius: 12px;
      color: #1976d2;
      font-weight: 600;
    }
    .votes-badge mat-icon {
      font-size: 14px;
      width: 14px;
      height: 14px;
    }
    
    .item-date {
      color: rgba(0,0,0,0.4);
    }
    
    /* Parking Card Admin */
    .parking-card-admin {
      margin-bottom: 24px;
    }
    .parking-content-admin {
      display: flex;
      gap: 40px;
      padding: 20px 0;
      align-items: center;
    }
    
    .parking-visual {
      flex-shrink: 0;
    }
    .parking-circle {
      position: relative;
      width: 180px;
      height: 180px;
    }
    .parking-circle svg {
      width: 100%;
      height: 100%;
      transform: rotate(0deg);
    }
    .parking-circle circle {
      transition: stroke-dashoffset 0.5s ease;
    }
    .parking-number {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      text-align: center;
    }
    .parking-number .number {
      font-size: 48px;
      font-weight: 700;
      color: #4caf50;
      line-height: 1;
    }
    .parking-number .label {
      font-size: 14px;
      color: rgba(0,0,0,0.6);
      margin-top: 4px;
    }
    
    .parking-details {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 20px;
    }
    .parking-stat {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px;
      background: #f5f5f5;
      border-radius: 12px;
    }
    .parking-stat mat-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
      color: #1976d2;
    }
    .stat-info {
      flex: 1;
    }
    .stat-value {
      font-size: 24px;
      font-weight: 700;
      color: rgba(0,0,0,0.87);
    }
    .stat-label {
      font-size: 13px;
      color: rgba(0,0,0,0.6);
      margin-top: 2px;
    }
    
    /* ============ ADHERENT DASHBOARD ============ */
    .message-card { margin-bottom: 24px; }
    .message-card.success { background: linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%); }
    .message-card.warning { background: linear-gradient(135deg, #fff3e0 0%, #ffe0b2 100%); }
    .message-card h3 {
      margin: 0;
      color: rgba(0,0,0,0.87);
    }
    
    .top-cards-grid { 
      display: grid; 
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); 
      gap: 16px; 
      margin-bottom: 24px; 
    }
    
    .contribution-card { height: 100%; }
    .contrib-row { display: flex; justify-content: space-between; padding: 8px 0; }
    .paid { color: green; font-weight: bold; }
    .warning { color: orange; font-weight: bold; }
    .status-badge { padding: 8px 16px; border-radius: 4px; text-align: center; margin-top: 16px; }
    .status-badge.uptodate { background: #4caf50; color: white; }
    .status-badge.late { background: #ff9800; color: white; }
    
    .parking-card { height: 100%; }
    .parking-content { 
      display: flex; 
      align-items: center; 
      gap: 24px; 
      padding: 16px 0; 
    }
    .parking-icon { 
      font-size: 72px; 
      width: 72px; 
      height: 72px; 
      color: #1976d2; 
    }
    .parking-info { flex: 1; }
    .parking-stats { 
      font-size: 48px; 
      font-weight: bold; 
      color: #1976d2; 
      line-height: 1; 
    }
    .parking-label { 
      font-size: 16px; 
      color: rgba(0, 0, 0, 0.6); 
      margin-top: 8px; 
    }
    .parking-total { 
      font-size: 14px; 
      color: rgba(0, 0, 0, 0.4); 
      margin-top: 4px; 
    }
    
    .ranking-card { margin-bottom: 24px; }
    .rank-item { display: flex; gap: 16px; padding: 12px; border-bottom: 1px solid #eee; align-items: center; }
    .rank-item.my-building { background: #e3f2fd; font-weight: bold; }
    .rank { font-size: 20px; font-weight: bold; width: 40px; }
    .building { flex: 1; }
    .percentage { font-weight: bold; color: #1976d2; }
    
    @media (max-width: 768px) {
      .top-cards-grid, .sections-grid { 
        grid-template-columns: 1fr; 
      }
      .parking-content-admin {
        flex-direction: column;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  loading = true;
  isAdmin = false;
  adminData?: AdminDashboard;
  adherentData?: AdherentDashboard;
  currentYear = new Date().getFullYear();

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    const user = this.authService.getCurrentUser();
    this.isAdmin = user ? (user.roles.includes('SuperAdmin') || user.roles.includes('Admin')) : false;

    this.dashboardService.getDashboard().subscribe({
      next: (data: any) => {
        if (this.isAdmin) {
          this.adminData = data as AdminDashboard;
        } else {
          this.adherentData = data as AdherentDashboard;
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('fr-MA', { style: 'currency', currency: 'MAD' }).format(amount);
  }

  formatAmountShort(amount: number): string {
    if (amount >= 1000000) {
      return (amount / 1000000).toFixed(1) + 'M';
    } else if (amount >= 1000) {
      return (amount / 1000).toFixed(1) + 'K';
    }
    return amount.toFixed(0);
  }

  getOccupancyRate(parking: any): number {
    if (parking.totalPlaces === 0) return 0;
    return Math.round((parking.occupiedPlaces / parking.totalPlaces) * 100);
  }
}
