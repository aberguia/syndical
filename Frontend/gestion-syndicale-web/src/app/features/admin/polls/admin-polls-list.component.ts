import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatMenuModule } from '@angular/material/menu';
import { FormsModule } from '@angular/forms';
import { PollsService } from '../../../core/services/polls.service';
import { Poll } from '../../../core/models/community.models';
import { AdminPollFormComponent } from './admin-poll-form.component';
import { AdminPollResultsComponent } from './admin-poll-results.component';

@Component({
  selector: 'app-admin-polls-list',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatFormFieldModule, 
            MatSelectModule, MatPaginatorModule, MatDialogModule, MatSnackBarModule,
            MatChipsModule, MatMenuModule, FormsModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Gestion des Sondages</h1>
        <button mat-raised-button color="primary" (click)="openForm()">
          <mat-icon>add</mat-icon>
          Nouveau Sondage
        </button>
      </div>

      <div class="filters">
        <mat-form-field>
          <mat-label>Statut</mat-label>
          <mat-select [(ngModel)]="statusFilter" (ngModelChange)="onFilterChange()">
            <mat-option value="">Tous</mat-option>
            <mat-option value="Draft">Brouillon</mat-option>
            <mat-option value="Published">Publié</mat-option>
            <mat-option value="Closed">Fermé</mat-option>
            <mat-option value="Archived">Archivé</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <div class="table-container">
        <table mat-table [dataSource]="polls" class="polls-table">
          <ng-container matColumnDef="question">
            <th mat-header-cell *matHeaderCellDef>Question</th>
            <td mat-cell *matCellDef="let poll">{{ poll.question }}</td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Statut</th>
            <td mat-cell *matCellDef="let poll">
              <mat-chip [class]="'status-' + poll.status.toLowerCase()">
                {{ getStatusLabel(poll.status) }}
              </mat-chip>
            </td>
          </ng-container>

          <ng-container matColumnDef="totalVotes">
            <th mat-header-cell *matHeaderCellDef>Votes</th>
            <td mat-cell *matCellDef="let poll">
              <strong>{{ getTotalVotes(poll) }}</strong>
            </td>
          </ng-container>

          <ng-container matColumnDef="createdOn">
            <th mat-header-cell *matHeaderCellDef>Créé le</th>
            <td mat-cell *matCellDef="let poll">
              {{ poll.createdOn | date:'dd/MM/yyyy HH:mm' }}
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let poll">
              <button mat-icon-button [matMenuTriggerFor]="menu" color="primary">
                <mat-icon>more_vert</mat-icon>
              </button>
              <mat-menu #menu="matMenu">
                <button mat-menu-item (click)="openForm(poll)" *ngIf="poll.status === 'Draft'">
                  <mat-icon>edit</mat-icon>
                  Modifier
                </button>
                <button mat-menu-item (click)="viewResults(poll)">
                  <mat-icon>bar_chart</mat-icon>
                  Voir les résultats
                </button>
                <button mat-menu-item (click)="publish(poll.id)" *ngIf="poll.status === 'Draft'">
                  <mat-icon>publish</mat-icon>
                  Publier
                </button>
                <button mat-menu-item (click)="close(poll.id)" *ngIf="poll.status === 'Published'">
                  <mat-icon>lock</mat-icon>
                  Fermer le vote
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
    .filters mat-form-field { max-width: 300px; }
    .table-container { background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
    .polls-table { width: 100%; }
    .status-draft { background-color: #9e9e9e; color: white; }
    .status-published { background-color: #4caf50; color: white; }
    .status-closed { background-color: #2196f3; color: white; }
    .status-archived { background-color: #ff9800; color: white; }
  `]
})
export class AdminPollsListComponent implements OnInit {
  polls: Poll[] = [];
  displayedColumns = ['question', 'status', 'totalVotes', 'createdOn', 'actions'];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  statusFilter = '';

  constructor(
    private pollsService: PollsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadPolls();
  }

  loadPolls() {
    this.pollsService.getAdminList(this.statusFilter, this.pageIndex + 1, this.pageSize).subscribe({
      next: (data) => {
        this.polls = data.items;
        this.totalCount = data.totalCount;
      },
      error: () => this.snackBar.open('Erreur lors du chargement', 'Fermer', { duration: 3000 })
    });
  }

  onFilterChange() {
    this.pageIndex = 0;
    this.loadPolls();
  }

  onPageChange(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadPolls();
  }

  openForm(poll?: Poll) {
    const dialogRef = this.dialog.open(AdminPollFormComponent, {
      width: '700px',
      data: poll
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadPolls();
      }
    });
  }

  viewResults(poll: Poll) {
    this.dialog.open(AdminPollResultsComponent, {
      width: '600px',
      data: poll
    });
  }

  publish(id: number) {
    this.pollsService.publish(id).subscribe({
      next: () => {
        this.snackBar.open('Sondage publié', 'Fermer', { duration: 3000 });
        this.loadPolls();
      },
      error: () => this.snackBar.open('Erreur lors de la publication', 'Fermer', { duration: 3000 })
    });
  }

  close(id: number) {
    if (confirm('Êtes-vous sûr de vouloir fermer ce sondage ? Les adhérents ne pourront plus voter.')) {
      this.pollsService.close(id).subscribe({
        next: () => {
          this.snackBar.open('Sondage fermé', 'Fermer', { duration: 3000 });
          this.loadPolls();
        },
        error: () => this.snackBar.open('Erreur lors de la fermeture', 'Fermer', { duration: 3000 })
      });
    }
  }

  getTotalVotes(poll: Poll): number {
    return poll.results?.reduce((sum, r) => sum + r.voteCount, 0) || 0;
  }

  getStatusLabel(status: string): string {
    const labels: any = {
      'Draft': 'Brouillon',
      'Published': 'Publié',
      'Closed': 'Fermé',
      'Archived': 'Archivé'
    };
    return labels[status] || status;
  }
}
