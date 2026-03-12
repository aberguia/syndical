import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { BuildingService } from '../../../core/services/building.service';
import { Building, BuildingDto } from '../../../core/models/settings.models';

@Component({
  selector: 'app-building-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSlideToggleModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data ? 'Modifier' : 'Ajouter' }} un immeuble</h2>
    
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Code immeuble *</mat-label>
          <input matInput formControlName="buildingNumber" placeholder="A, B, 1...">
          <mat-error *ngIf="form.get('buildingNumber')?.hasError('required')">
            Le code est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Nom *</mat-label>
          <input matInput formControlName="name" placeholder="Bâtiment A">
          <mat-error *ngIf="form.get('name')?.hasError('required')">
            Le nom est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Nombre d'étages *</mat-label>
          <input matInput type="number" formControlName="floorCount" placeholder="5">
          <mat-error *ngIf="form.get('floorCount')?.hasError('required')">
            Le nombre d'étages est requis
          </mat-error>
          <mat-error *ngIf="form.get('floorCount')?.hasError('min')">
            Minimum 1 étage
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
      min-width: 400px;
      padding: 20px 24px;
    }
  `]
})
export class BuildingDialogComponent implements OnInit {
  form: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private buildingService: BuildingService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<BuildingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Building | null
  ) {
    this.form = this.fb.group({
      buildingNumber: ['', [Validators.required]],
      name: ['', [Validators.required]],
      floorCount: [1, [Validators.required, Validators.min(1)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    if (this.data) {
      this.form.patchValue({
        buildingNumber: this.data.buildingNumber,
        name: this.data.name,
        floorCount: this.data.floorCount,
        isActive: this.data.isActive
      });
    }
  }

  onSave(): void {
    if (this.form.invalid) return;

    this.saving = true;
    const buildingDto: BuildingDto = this.form.value;

    const request = this.data
      ? this.buildingService.update(this.data.id, buildingDto)
      : this.buildingService.create(buildingDto);

    request.subscribe({
      next: () => {
        this.snackBar.open(
          this.data ? 'Immeuble modifié avec succès' : 'Immeuble créé avec succès',
          'Fermer',
          { duration: 3000 }
        );
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error saving building:', error);
        this.snackBar.open(
          error.error?.message || 'Erreur lors de l\'enregistrement',
          'Fermer',
          { duration: 5000 }
        );
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
