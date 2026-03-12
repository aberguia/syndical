import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatIconModule } from '@angular/material/icon';
import { PollsService } from '../../../core/services/polls.service';
import { Poll, PollResult } from '../../../core/models/community.models';

@Component({
  selector: 'app-admin-poll-results',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatProgressBarModule, MatIconModule],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>bar_chart</mat-icon>
      Résultats du sondage
    </h2>
    
    <mat-dialog-content>
      <h3 class="question">{{ poll.question }}</h3>
      
      <div class="total-votes">
        <mat-icon>how_to_vote</mat-icon>
        <span><strong>{{ totalVotes }}</strong> vote(s) au total</span>
      </div>

      <div class="results">
        <div *ngFor="let result of results" class="result-item">
          <div class="result-header">
            <span class="option-label">{{ result.label }}</span>
            <span class="votes-count">{{ result.voteCount }} vote(s) - {{ result.percentage.toFixed(1) }}%</span>
          </div>
          <mat-progress-bar mode="determinate" [value]="result.percentage" 
                           [class]="'bar-' + getBarColor(result.percentage)">
          </mat-progress-bar>
        </div>

        <div *ngIf="results.length === 0" class="no-votes">
          <mat-icon>info</mat-icon>
          <p>Aucun vote pour le moment</p>
        </div>
      </div>

      <div class="status-info">
        <mat-icon>info</mat-icon>
        <span>Statut : <strong>{{ getStatusLabel(poll.status) }}</strong></span>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Fermer</button>
    </mat-dialog-actions>
  `,
  styles: [`
    h2 mat-icon { vertical-align: middle; margin-right: 8px; }
    .question { color: #333; margin-bottom: 16px; font-weight: 500; }
    .total-votes { display: flex; align-items: center; gap: 8px; padding: 12px; background: #f5f5f5; border-radius: 8px; margin-bottom: 24px; }
    .total-votes mat-icon { color: #1976d2; }
    .results { margin-bottom: 24px; }
    .result-item { margin-bottom: 20px; }
    .result-header { display: flex; justify-content: space-between; margin-bottom: 8px; }
    .option-label { font-weight: 500; color: #333; }
    .votes-count { color: #666; font-size: 14px; }
    mat-progress-bar { height: 24px; border-radius: 4px; }
    .bar-high ::ng-deep .mat-mdc-progress-bar-fill::after { background-color: #4caf50 !important; }
    .bar-medium ::ng-deep .mat-mdc-progress-bar-fill::after { background-color: #2196f3 !important; }
    .bar-low ::ng-deep .mat-mdc-progress-bar-fill::after { background-color: #ff9800 !important; }
    .no-votes { text-align: center; padding: 32px; color: #999; }
    .no-votes mat-icon { font-size: 48px; width: 48px; height: 48px; }
    .status-info { display: flex; align-items: center; gap: 8px; padding: 12px; background: #e3f2fd; border-radius: 8px; color: #1976d2; }
  `]
})
export class AdminPollResultsComponent implements OnInit {
  results: PollResult[] = [];
  totalVotes = 0;

  constructor(
    @Inject(MAT_DIALOG_DATA) public poll: Poll,
    private pollsService: PollsService
  ) {}

  ngOnInit() {
    this.loadResults();
  }

  loadResults() {
    this.pollsService.getResults(this.poll.id).subscribe({
      next: (data) => {
        this.results = data;
        this.totalVotes = data.reduce((sum, r) => sum + r.voteCount, 0);
      }
    });
  }

  getBarColor(percentage: number): string {
    if (percentage >= 50) return 'high';
    if (percentage >= 20) return 'medium';
    return 'low';
  }

  getStatusLabel(status: string): string {
    const labels: any = {
      'Draft': 'Brouillon',
      'Published': 'Publié (en cours)',
      'Closed': 'Fermé',
      'Archived': 'Archivé'
    };
    return labels[status] || status;
  }
}
