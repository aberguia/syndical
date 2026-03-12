import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApartmentService } from '../../../core/services/apartment.service';
import { Apartment, ApartmentDto, Building } from '../../../core/models/settings.models';

@Component({
  selector: 'app-apartment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatSlideToggleModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.apartment ? 'Modifier' : 'Ajouter' }} un appartement</h2>
    
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Immeuble *</mat-label>
          <mat-select formControlName="buildingId">
            <mat-option *ngFor="let building of data.buildings" [value]="building.id">
              {{ building.buildingNumber }} - {{ building.name }}
            </mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('buildingId')?.hasError('required')">
            L'immeuble est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Numéro d'appartement *</mat-label>
          <input matInput formControlName="apartmentNumber" placeholder="101, A1...">
          <mat-error *ngIf="form.get('apartmentNumber')?.hasError('required')">
            Le numéro est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Étage *</mat-label>
          <input matInput type="number" formControlName="floor" placeholder="1">
          <mat-error *ngIf="form.get('floor')?.hasError('required')">
            L'étage est requis
          </mat-error>
          <mat-error *ngIf="form.get('floor')?.hasError('min')">
            Minimum 0 (RDC)
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Surface (m²)</mat-label>
          <input matInput type="number" step="0.01" formControlName="surface" placeholder="65.5">
          <mat-error *ngIf="form.get('surface')?.hasError('min')">
            La surface doit être positive
          </mat-error>
        </mat-form-field>

        <mat-slide-toggle formControlName="isActive" color="primary">
          Actif
        </mat-slide-toggle>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Annuler</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="form.invalid || saving">
        {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    mat-dialog-content {
      min-width: 500px;
      padding: 20px 24px;
    }
  `]
})
export class ApartmentDialogComponent implements OnInit {
  form: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private apartmentService: ApartmentService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ApartmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { apartment: Apartment | null; buildings: Building[] }
  ) {
    this.form = this.fb.group({
      buildingId: ['', [Validators.required]],
      apartmentNumber: ['', [Validators.required]],
      floor: [0, [Validators.required, Validators.min(0)]],
      surface: [null, [Validators.min(0)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    if (this.data.apartment) {
      this.form.patchValue({
        buildingId: this.data.apartment.buildingId,
        apartmentNumber: this.data.apartment.apartmentNumber,
        floor: this.data.apartment.floor,
        surface: this.data.apartment.surface,
        isActive: this.data.apartment.isActive
      });
    }
  }

  onSave(): void {
    if (this.form.invalid) return;

    this.saving = true;
    const apartmentDto: ApartmentDto = { ...this.form.value, sharesCount: 100 };

    const request = this.data.apartment
      ? this.apartmentService.update(this.data.apartment.id, apartmentDto)
      : this.apartmentService.create(apartmentDto);

    request.subscribe({
      next: () => {
        this.snackBar.open(
          this.data.apartment ? 'Appartement modifié avec succès' : 'Appartement créé avec succès',
          'Fermer',
          { duration: 3000 }
        );
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error saving apartment:', error);
        let message = 'Erreur lors de l\'enregistrement';
        if (error.status === 409 || error.error?.message?.includes('existe déjà')) {
          message = 'Cet appartement existe déjà pour cet immeuble';
        }
        this.snackBar.open(message, 'Fermer', { duration: 5000 });
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
