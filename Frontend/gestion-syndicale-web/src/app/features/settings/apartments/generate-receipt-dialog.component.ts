import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Apartment } from '../../../core/models/settings.models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

export interface ReceiptDialogData {
  apartment: Apartment;
  memberName?: string;
}

@Component({
  selector: 'app-generate-receipt-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon class="dialog-icon">receipt_long</mat-icon>
      Générer un reçu - Appartement {{ data.apartment.buildingNumber }} / {{ data.apartment.apartmentNumber }}
    </h2>
    
    <mat-dialog-content>
      <div *ngIf="!data.memberName" class="error-message">
        <mat-icon>warning</mat-icon>
        <p>Aucun adhérent associé à cet appartement, impossible de générer un reçu.</p>
      </div>

      <div *ngIf="data.memberName" class="apartment-info">
        <p><strong>Adhérent :</strong> {{ data.memberName }}</p>
        <p><strong>Immeuble :</strong> {{ data.apartment.buildingNumber }} - {{ data.apartment.buildingName }}</p>
        <p><strong>Appartement :</strong> {{ data.apartment.apartmentNumber }}</p>
      </div>

      <mat-form-field *ngIf="data.memberName" appearance="outline" class="full-width">
        <mat-label>Années *</mat-label>
        <mat-select [formControl]="yearsControl" multiple>
          <mat-option *ngFor="let year of availableYears" [value]="year">
            {{ year }}
          </mat-option>
        </mat-select>
        <mat-hint>Sélectionnez une ou plusieurs années</mat-hint>
      </mat-form-field>

      <div *ngIf="generating" class="loading-container">
        <mat-spinner diameter="40"></mat-spinner>
        <p>Génération du reçu en cours...</p>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="generating">Annuler</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onGenerate()"
        [disabled]="!data.memberName || yearsControl.invalid || generating">
        <mat-icon>download</mat-icon>
        Générer PDF
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-icon {
      vertical-align: middle;
      margin-right: 8px;
    }

    mat-dialog-content {
      min-width: 400px;
      padding: 20px 24px;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      background-color: #ffebee;
      border-radius: 4px;
      color: #c62828;
      margin-bottom: 16px;

      mat-icon {
        color: #c62828;
      }

      p {
        margin: 0;
      }
    }

    .apartment-info {
      padding: 16px;
      background-color: #f5f5f5;
      border-radius: 4px;
      margin-bottom: 20px;

      p {
        margin: 8px 0;
        font-size: 14px;

        strong {
          font-weight: 600;
        }
      }
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 16px;
      padding: 20px;

      p {
        margin: 0;
        color: #666;
      }
    }

    mat-dialog-actions {
      padding: 16px 24px;

      button mat-icon {
        margin-right: 4px;
      }
    }
  `]
})
export class GenerateReceiptDialogComponent implements OnInit {
  yearsControl: FormControl;
  availableYears: number[] = [];
  generating = false;
  private readonly RESIDENCE_START_YEAR = 2022;

  constructor(
    public dialogRef: MatDialogRef<GenerateReceiptDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ReceiptDialogData,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {
    this.yearsControl = new FormControl([], [Validators.required, Validators.minLength(1)]);
  }

  ngOnInit(): void {
    this.initializeYears();
  }

  initializeYears(): void {
    const currentYear = new Date().getFullYear();
    this.availableYears = [];
    
    for (let year = this.RESIDENCE_START_YEAR; year <= currentYear; year++) {
      this.availableYears.push(year);
    }

    // Sélection par défaut : année actuelle
    this.yearsControl.setValue([currentYear]);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onGenerate(): void {
    if (this.yearsControl.invalid || !this.data.memberName) {
      return;
    }

    this.generating = true;
    const years = this.yearsControl.value as number[];
    const apartmentId = this.data.apartment.id;

    this.http.post(
      `${environment.apiUrl}/receipts/apartment/${apartmentId}`,
      { years },
      { responseType: 'blob', observe: 'response' }
    ).subscribe({
      next: (response) => {
        this.generating = false;
        
        // Extraire le nom du fichier depuis le header Content-Disposition
        let filename = this.generateFileName(years);
        const contentDisposition = response.headers.get('Content-Disposition');
        if (contentDisposition) {
          const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
          if (matches && matches[1]) {
            filename = matches[1].replace(/['"]/g, '');
          }
        }

        // Télécharger le fichier
        const blob = response.body;
        if (blob) {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = filename;
          link.click();
          window.URL.revokeObjectURL(url);

          this.snackBar.open('Reçu généré avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        }
      },
      error: (error) => {
        this.generating = false;
        console.error('Error generating receipt:', error);
        this.snackBar.open('Impossible de générer le reçu', 'Fermer', { duration: 5000 });
      }
    });
  }

  private generateFileName(years: number[]): string {
    const buildingCode = this.data.apartment.buildingNumber || 'B';
    const apartmentNumber = this.data.apartment.apartmentNumber || 'A';
    const yearsSuffix = years.sort().join('_');
    return `recu_${buildingCode}_${apartmentNumber}_${yearsSuffix}.pdf`;
  }
}
