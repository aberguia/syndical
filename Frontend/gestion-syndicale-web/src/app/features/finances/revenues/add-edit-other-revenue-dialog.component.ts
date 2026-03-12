import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RevenuesService } from '../../../core/services/revenues.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-add-edit-other-revenue-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule
  ],
  template: `
    <h2 mat-dialog-title>{{ isEditMode ? 'Modifier le revenu' : 'Ajouter un revenu' }}</h2>

    <mat-dialog-content>
      <form [formGroup]="form" class="revenue-form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Date</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="revenueDate" required>
          <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
          <mat-error *ngIf="form.get('revenueDate')?.hasError('required')">
            La date est obligatoire
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Titre</mat-label>
          <input matInput formControlName="title" placeholder="Ex: Don généreux, Intérêts bancaires..." required>
          <mat-error *ngIf="form.get('title')?.hasError('required')">
            Le titre est obligatoire
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Montant</mat-label>
          <input matInput type="number" formControlName="amount" placeholder="0.00" required>
          <span matPrefix>MAD&nbsp;</span>
          <mat-error *ngIf="form.get('amount')?.hasError('required')">
            Le montant est obligatoire
          </mat-error>
          <mat-error *ngIf="form.get('amount')?.hasError('min')">
            Le montant doit être supérieur à 0
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="4" 
                    placeholder="Détails supplémentaires..."></textarea>
        </mat-form-field>
      </form>

      <div class="loading-overlay" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="loading">Annuler</button>
      <button mat-raised-button color="primary" (click)="onSubmit()" 
              [disabled]="form.invalid || loading">
        {{ isEditMode ? 'Modifier' : 'Créer' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .revenue-form {
      display: flex;
      flex-direction: column;
      gap: 16px;
      min-width: 500px;
      padding: 16px 0;
    }

    .full-width {
      width: 100%;
    }

    .loading-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(255, 255, 255, 0.8);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 10;
    }

    mat-dialog-content {
      position: relative;
      min-height: 400px;
    }
  `]
})
export class AddEditOtherRevenueDialogComponent implements OnInit {
  form: FormGroup;
  loading = false;
  isEditMode = false;
  revenueId?: number;

  constructor(
    private fb: FormBuilder,
    private revenuesService: RevenuesService,
    private snackBar: MatSnackBar,
    private dialogRef: MatDialogRef<AddEditOtherRevenueDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.form = this.fb.group({
      revenueDate: [null, Validators.required],
      title: ['', Validators.required],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      description: ['']
    });
  }

  ngOnInit(): void {
    if (this.data?.mode === 'edit' && this.data?.revenueId) {
      this.isEditMode = true;
      this.revenueId = this.data.revenueId;
      this.loadRevenue();
    } else {
      // Mode création - initialiser avec la date du jour
      this.form.patchValue({
        revenueDate: new Date(),
        amount: 0
      });
    }
  }

  loadRevenue(): void {
    if (!this.revenueId) return;

    this.loading = true;
    this.revenuesService.getOtherRevenue(this.revenueId).subscribe({
      next: (revenue) => {
        console.log('Revenue loaded:', revenue);
        this.form.patchValue({
          revenueDate: new Date(revenue.revenueDate),
          title: revenue.title,
          amount: revenue.amount,
          description: revenue.description || ''
        });
        console.log('Form values after patch:', this.form.value);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading revenue:', error);
        this.snackBar.open('Erreur lors du chargement', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.loading = true;
    const formValue = this.form.value;

    const dto = {
      revenueDate: formValue.revenueDate,
      title: formValue.title,
      amount: formValue.amount,
      description: formValue.description || null
    };

    const request = this.isEditMode && this.revenueId
      ? this.revenuesService.updateOtherRevenue(this.revenueId, dto)
      : this.revenuesService.createOtherRevenue(dto);

    request.subscribe({
      next: () => {
        this.snackBar.open(
          this.isEditMode ? 'Revenu modifié avec succès' : 'Revenu créé avec succès',
          'Fermer',
          { duration: 3000 }
        );
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error saving revenue:', error);
        this.snackBar.open('Erreur lors de l\'enregistrement', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
