import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatRadioModule } from '@angular/material/radio';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatChipsModule } from '@angular/material/chips';
import { FormsModule } from '@angular/forms';
import { PollsService } from '../../../core/services/polls.service';
import { PortalPoll, PollVoteDto } from '../../../core/models/community.models';

@Component({
  selector: 'app-portal-polls',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatRadioModule, 
            MatProgressBarModule, MatProgressSpinnerModule, MatSnackBarModule, MatChipsModule, FormsModule],
  template: `
    <div class="page-container">
      <h1>Sondages</h1>
      
      <div *ngIf="loading" class="loading-container">
        <mat-spinner></mat-spinner>
      </div>

      <div *ngIf="!loading" class="polls-container">
        <mat-card *ngFor="let poll of polls" class="poll-card">
          <mat-card-header>
            <mat-card-title>{{ poll.question }}</mat-card-title>
            <mat-card-subtitle>
              <mat-chip [class]="'status-' + poll.status.toLowerCase()">
                {{ getStatusLabel(poll.status) }}
              </mat-chip>
            </mat-card-subtitle>
          </mat-card-header>

          <mat-card-content>
            <!-- Interface de vote -->
            <div *ngIf="poll.status === 'Published'" class="vote-section">
              <h3>Votez pour une option :</h3>
              <mat-radio-group [(ngModel)]="poll.selectedOptionId" class="radio-group">
                <mat-radio-button *ngFor="let option of poll.options" 
                                  [value]="option.id" 
                                  class="radio-option">
                  {{ option.label }}
                </mat-radio-button>
              </mat-radio-group>
              
              <button mat-raised-button color="primary" 
                      (click)="vote(poll)" 
                      [disabled]="!poll.selectedOptionId || voting === poll.id">
                <mat-icon>how_to_vote</mat-icon>
                {{ poll.hasVoted ? 'Modifier mon vote' : 'Voter' }}
              </button>
              
              <p *ngIf="poll.hasVoted" class="voted-info">
                <mat-icon>check_circle</mat-icon>
                Vous avez déjà voté pour ce sondage
              </p>
            </div>

            <!-- Résultats -->
            <div class="results-section">
              <h3>
                <mat-icon>bar_chart</mat-icon>
                Résultats en temps réel
              </h3>
              
              <div class="total-votes">
                <strong>{{ getTotalVotes(poll) }}</strong> vote(s)
              </div>

              <div *ngFor="let result of poll.results" class="result-item">
                <div class="result-header">
                  <span class="option-label">
                    {{ result.label }}
                    <mat-icon *ngIf="poll.myVoteOptionId === result.optionId" class="my-vote">
                      how_to_vote
                    </mat-icon>
                  </span>
                  <span class="percentage">{{ result.voteCount }} ({{ result.percentage.toFixed(1) }}%)</span>
                </div>
                <mat-progress-bar mode="determinate" 
                                 [value]="result.percentage"
                                 [class]="poll.myVoteOptionId === result.optionId ? 'my-vote-bar' : ''">
                </mat-progress-bar>
              </div>
            </div>

            <div *ngIf="poll.status === 'Closed'" class="closed-info">
              <mat-icon>lock</mat-icon>
              <span>Ce sondage est fermé, vous ne pouvez plus voter</span>
            </div>
          </mat-card-content>

          <mat-card-footer>
            <div class="footer-info">
              <mat-icon>calendar_today</mat-icon>
              <span>Créé le {{ poll.createdOn | date:'dd/MM/yyyy à HH:mm' }}</span>
            </div>
          </mat-card-footer>
        </mat-card>

        <div *ngIf="polls.length === 0" class="no-data">
          <mat-icon>info</mat-icon>
          <p>Aucun sondage disponible pour le moment</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 24px; max-width: 900px; margin: 0 auto; }
    h1 { color: #333; margin-bottom: 24px; }
    .loading-container { display: flex; justify-content: center; padding: 48px; }
    .polls-container { display: flex; flex-direction: column; gap: 24px; }
    .poll-card { padding: 24px; }
    .status-published { background-color: #4caf50; color: white; }
    .status-closed { background-color: #2196f3; color: white; }
    
    .vote-section { margin-bottom: 32px; padding-bottom: 24px; border-bottom: 1px solid #eee; }
    .vote-section h3 { margin-bottom: 16px; color: #333; }
    .radio-group { display: flex; flex-direction: column; gap: 12px; margin-bottom: 16px; }
    .radio-option { margin: 4px 0; }
    .voted-info { display: flex; align-items: center; gap: 8px; color: #4caf50; margin-top: 12px; font-size: 14px; }
    .voted-info mat-icon { font-size: 18px; width: 18px; height: 18px; }
    
    .results-section h3 { display: flex; align-items: center; gap: 8px; margin-bottom: 16px; color: #333; }
    .total-votes { text-align: center; padding: 12px; background: #f5f5f5; border-radius: 8px; margin-bottom: 16px; font-size: 18px; }
    .result-item { margin-bottom: 16px; }
    .result-header { display: flex; justify-content: space-between; margin-bottom: 8px; }
    .option-label { font-weight: 500; color: #333; display: flex; align-items: center; gap: 4px; }
    .my-vote { color: #1976d2; font-size: 20px; }
    .percentage { color: #666; }
    mat-progress-bar { height: 20px; border-radius: 4px; }
    .my-vote-bar ::ng-deep .mat-mdc-progress-bar-fill::after { background-color: #1976d2 !important; }
    
    .closed-info { display: flex; align-items: center; gap: 8px; padding: 12px; background: #fff3cd; border-radius: 8px; color: #856404; margin-top: 16px; }
    .footer-info { display: flex; align-items: center; gap: 8px; padding: 16px; color: #666; font-size: 14px; }
    .footer-info mat-icon { font-size: 18px; width: 18px; height: 18px; }
    
    .no-data { text-align: center; padding: 48px; color: #999; }
    .no-data mat-icon { font-size: 64px; width: 64px; height: 64px; }
  `]
})
export class PortalPollsComponent implements OnInit {
  polls: PortalPoll[] = [];
  loading = false;
  voting: number | null = null;

  constructor(
    private pollsService: PollsService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.loadPolls();
  }

  loadPolls() {
    this.loading = true;
    this.pollsService.getPublished().subscribe({
      next: (data) => {
        this.polls = data.map(poll => ({
          ...poll,
          selectedOptionId: poll.myVoteOptionId || poll.options[0]?.id
        }));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Erreur lors du chargement', 'Fermer', { duration: 3000 });
      }
    });
  }

  vote(poll: PortalPoll) {
    if (!poll.selectedOptionId) {
      this.snackBar.open('Veuillez sélectionner une option', 'Fermer', { duration: 3000 });
      return;
    }

    this.voting = poll.id;
    const voteDto: PollVoteDto = {
      pollId: poll.id,
      pollOptionId: poll.selectedOptionId
    };

    this.pollsService.vote(voteDto).subscribe({
      next: () => {
        this.snackBar.open(
          poll.hasVoted ? 'Votre vote a été modifié' : 'Merci pour votre vote !', 
          'Fermer', 
          { duration: 3000 }
        );
        this.voting = null;
        this.loadPolls(); // Recharger pour mettre à jour les résultats
      },
      error: (err) => {
        this.voting = null;
        const message = err.error?.message || 'Erreur lors du vote';
        this.snackBar.open(message, 'Fermer', { duration: 3000 });
      }
    });
  }

  getTotalVotes(poll: PortalPoll): number {
    return poll.results.reduce((sum, r) => sum + r.voteCount, 0);
  }

  getStatusLabel(status: string): string {
    const labels: any = {
      'Published': 'En cours',
      'Closed': 'Fermé'
    };
    return labels[status] || status;
  }
}
