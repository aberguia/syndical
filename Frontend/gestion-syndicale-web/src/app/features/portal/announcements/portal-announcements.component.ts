import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AnnouncementsService } from '../../../core/services/announcements.service';
import { Announcement, AnnouncementListDto } from '../../../core/models/community.models';

@Component({
  selector: 'app-portal-announcements',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, 
            MatPaginatorModule, MatDialogModule, MatProgressSpinnerModule],
  template: `
    <div class="page-container">
      <h1>Annonces</h1>
      
      <div *ngIf="loading" class="loading-container">
        <mat-spinner></mat-spinner>
      </div>

      <div *ngIf="!loading" class="announcements-grid">
        <mat-card *ngFor="let announcement of announcements" class="announcement-card">
          <mat-card-header>
            <mat-card-title>{{ announcement.title }}</mat-card-title>
            <mat-card-subtitle>
              {{ announcement.createdOn | date:'dd/MM/yyyy à HH:mm' }} - 
              Par {{ announcement.createdByName }}
            </mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content>
            <p class="announcement-preview">{{ getPreview(announcement.body) }}</p>
          </mat-card-content>
          
          <mat-card-actions align="end">
            <button mat-button color="primary" (click)="viewDetails(announcement)">
              <mat-icon>visibility</mat-icon>
              Lire la suite
            </button>
          </mat-card-actions>
        </mat-card>

        <div *ngIf="announcements.length === 0" class="no-data">
          <mat-icon>info</mat-icon>
          <p>Aucune annonce disponible pour le moment</p>
        </div>
      </div>

      <mat-paginator *ngIf="!loading && totalCount > 0"
        [length]="totalCount" 
        [pageSize]="pageSize" 
        [pageIndex]="pageIndex"
        [pageSizeOptions]="[6, 12, 24]" 
        (page)="onPageChange($event)">
      </mat-paginator>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 1200px; margin: 0 auto; }
    h1 { color: #333; margin-bottom: 24px; }
    .loading-container { display: flex; justify-content: center; padding: 48px; }
    .announcements-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); gap: 24px; margin-bottom: 24px; }
    .announcement-card { cursor: pointer; transition: transform 0.2s, box-shadow 0.2s; }
    .announcement-card:hover { transform: translateY(-4px); box-shadow: 0 4px 12px rgba(0,0,0,0.15); }
    .announcement-preview { color: #666; overflow: hidden; text-overflow: ellipsis; display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; }
    .no-data { text-align: center; padding: 48px; color: #999; }
    .no-data mat-icon { font-size: 64px; width: 64px; height: 64px; }
  `]
})
export class PortalAnnouncementsComponent implements OnInit {
  announcements: Announcement[] = [];
  loading = false;
  totalCount = 0;
  pageSize = 6;
  pageIndex = 0;

  constructor(
    private announcementsService: AnnouncementsService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.loadAnnouncements();
  }

  loadAnnouncements() {
    this.loading = true;
    this.announcementsService.getPublished(this.pageIndex + 1, this.pageSize).subscribe({
      next: (data) => {
        this.announcements = data;
        this.totalCount = data.length;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadAnnouncements();
  }

  getPreview(body: string): string {
    return body.length > 150 ? body.substring(0, 150) + '...' : body;
  }

  viewDetails(announcement: Announcement) {
    this.dialog.open(AnnouncementDetailDialog, {
      width: '700px',
      data: announcement
    });
  }
}

@Component({
  selector: 'app-announcement-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    
    <mat-dialog-content>
      <div class="meta">
        <mat-icon>calendar_today</mat-icon>
        <span>{{ data.createdOn | date:'dd/MM/yyyy à HH:mm' }}</span>
        <mat-icon>person</mat-icon>
        <span>{{ data.createdByName }}</span>
      </div>
      <div class="content">
        {{ data.body }}
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Fermer</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .meta { display: flex; align-items: center; gap: 8px; color: #666; margin-bottom: 24px; font-size: 14px; }
    .meta mat-icon { font-size: 18px; width: 18px; height: 18px; }
    .content { white-space: pre-wrap; line-height: 1.6; color: #333; }
  `]
})
export class AnnouncementDetailDialog {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Announcement) {}
}
