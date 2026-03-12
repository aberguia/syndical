import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { FormsModule } from '@angular/forms';
import { AnnouncementsService } from '../../../core/services/announcements.service';
import { Announcement, AnnouncementListDto } from '../../../core/models/community.models';
import { AdminAnnouncementFormComponent } from './admin-announcement-form.component';

@Component({
  selector: 'app-admin-announcements-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, 
            MatInputModule, MatSelectModule, MatPaginatorModule, MatDialogModule, MatSnackBarModule,
            MatChipsModule, MatMenuModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Gestion des Annonces</h1>
        <button mat-raised-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon>
          Nouvelle Annonce
        </button>
      </div>

      <div class="filters">
        <mat-form-field>
          <mat-label>Rechercher</mat-label>
          <input matInput [(ngModel)]="searchTerm" (ngModelChange)="onSearch()" placeholder="Titre...">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>

        <mat-form-field>
          <mat-label>Statut</mat-label>
          <mat-select [(ngModel)]="statusFilter" (ngModelChange)="onFilterChange()">
            <mat-option value="">Tous</mat-option>
            <mat-option value="Draft">Brouillon</mat-option>
            <mat-option value="Published">Publié</mat-option>
            <mat-option value="Archived">Archivé</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="announcements" class="announcements-table">
          <ng-container matColumnDef="title">
            <th mat-header-cell *matHeaderCellDef>Titre</th>
            <td mat-cell *matCellDef="let announcement">{{ announcement.title }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Statut</th>
            <td mat-cell *matCellDef="let announcement">
              <mat-chip [class]="'status-' + announcement.status.toLowerCase()">
                {{ getStatusLabel(announcement.status) }}
              </mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="createdBy">
            <th mat-header-cell *matHeaderCellDef>Créé par</th>
            <td mat-cell *matCellDef="let announcement">
              {{ announcement.createdByFirstName }} {{ announcement.createdByLastName }}
            </td>
          </ng-container>

          <ng-container matColumnDef="createdOn">
            <th mat-header-cell *matHeaderCellDef>Date de création</th>
            <td mat-cell *matCellDef="let announcement">
              {{ announcement.createdOn | date:'dd/MM/yyyy HH:mm' }}
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let announcement">
              <button mat-icon-button [matMenuTriggerFor]="menu" color="primary">
                <mat-icon>more_vert</mat-icon>
              </button>
              <mat-menu #menu="matMenu">
                <button mat-menu-item (click)="openForm(announcement)">
                  <mat-icon>edit</mat-icon>
                  Modifier
                </button>
                <button mat-menu-item (click)="publish(announcement.id)" *ngIf="announcement.status === 'Draft'">
                  <mat-icon>publish</mat-icon>
                  Publier
                </button>
                <button mat-menu-item (click)="archive(announcement.id)" *ngIf="announcement.status === 'Published'">
                  <mat-icon>archive</mat-icon>
                  Archiver
                </button>
                <button mat-menu-item (click)="delete(announcement.id)" class="delete-button">
                  <mat-icon>delete</mat-icon>
                  Supprimer
                </button>
              </mat-menu>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>

        <mat-paginator [length]="totalCount" [pageSize]="pageSize" [pageIndex]="pageIndex"
          [pageSizeOptions]="[10, 20, 50]" (page)="onPageChange($event)">
        </mat-paginator>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .filters { display: flex; gap: 16px; margin-bottom: 24px; }
    .filters mat-form-field { flex: 1; max-width: 300px; }
    .table-container { background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    .announcements-table { width: 100%; }
    .status-draft { background-color: #9e9e9e; color: white; }
    .status-published { background-color: #4caf50; color: white; }
    .status-archived { background-color: #ff9800; color: white; }
    .delete-button { color: #f44336; }
  `]
})
export class AdminAnnouncementsListComponent implements OnInit {
  announcements: Announcement[] = [];
  displayedColumns = ['title', 'status', 'createdBy', 'createdOn', 'actions'];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  searchTerm = '';
  statusFilter = '';

  constructor(
    private announcementsService: AnnouncementsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadAnnouncements();
  }

  loadAnnouncements() {
    this.announcementsService.getAdminList(this.statusFilter, this.searchTerm, this.pageIndex + 1, this.pageSize)
      .subscribe({
        next: (data: AnnouncementListDto) => {
          this.announcements = data.items;
          this.totalCount = data.totalCount;
        },
        error: () => this.snackBar.open('Erreur lors du chargement', 'Fermer', { duration: 3000 })
      });
  }

  onSearch() {
    this.pageIndex = 0;
    this.loadAnnouncements();
  }

  onFilterChange() {
    this.pageIndex = 0;
    this.loadAnnouncements();
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadAnnouncements();
  }

  openForm(announcement?: Announcement) {
    const dialogRef = this.dialog.open(AdminAnnouncementFormComponent, {
      width: '700px',
      data: announcement
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadAnnouncements();
      }
    });
  }

  publish(id: number) {
    this.announcementsService.publish(id).subscribe({
      next: () => {
        this.snackBar.open('Annonce publiée', 'Fermer', { duration: 3000 });
        this.loadAnnouncements();
      },
      error: () => this.snackBar.open('Erreur lors de la publication', 'Fermer', { duration: 3000 })
    });
  }

  archive(id: number) {
    this.announcementsService.archive(id).subscribe({
      next: () => {
        this.snackBar.open('Annonce archivée', 'Fermer', { duration: 3000 });
        this.loadAnnouncements();
      },
      error: () => this.snackBar.open('Erreur lors de l\'archivage', 'Fermer', { duration: 3000 })
    });
  }

  delete(id: number) {
    if (confirm('Êtes-vous sûr de vouloir supprimer cette annonce ?')) {
      this.announcementsService.delete(id).subscribe({
        next: () => {
          this.snackBar.open('Annonce supprimée', 'Fermer', { duration: 3000 });
          this.loadAnnouncements();
        },
        error: () => this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 })
      });
    }
  }

  getStatusLabel(status: string): string {
    const labels: any = {
      'Draft': 'Brouillon',
      'Published': 'Publié',
      'Archived': 'Archivé'
    };
    return labels[status] || status;
  }
}
