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
import { TranslateModule, TranslateService } from '@ngx-translate/core';
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
    MatSnackBarModule,
    TranslateModule
  ],
  template: `
    <h2 mat-dialog-title>{{ (data.apartment ? 'APARTMENTS.DIALOG_EDIT_TITLE' : 'APARTMENTS.DIALOG_ADD_TITLE') | translate }}</h2>

    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'APARTMENTS.FIELD_BUILDING' | translate }}</mat-label>
          <mat-select formControlName="buildingId">
            <mat-option *ngFor="let building of data.buildings" [value]="building.id">
              {{ building.buildingNumber }} - {{ building.name }}
            </mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('buildingId')?.hasError('required')">
            {{ 'COMMON.FIELD_REQUIRED' | translate }}
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'APARTMENTS.FIELD_NUMBER' | translate }}</mat-label>
          <input matInput formControlName="apartmentNumber" placeholder="101, A1...">
          <mat-error *ngIf="form.get('apartmentNumber')?.hasError('required')">
            {{ 'COMMON.FIELD_REQUIRED' | translate }}
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'APARTMENTS.FIELD_FLOOR' | translate }}</mat-label>
          <input matInput type="number" formControlName="floor" placeholder="1">
          <mat-error *ngIf="form.get('floor')?.hasError('required')">
            {{ 'COMMON.FIELD_REQUIRED' | translate }}
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'APARTMENTS.FIELD_SURFACE' | translate }}</mat-label>
          <input matInput type="number" step="0.01" formControlName="surface" placeholder="65.5">
        </mat-form-field>

        <mat-slide-toggle formControlName="isActive" color="primary">
          {{ 'APARTMENTS.FIELD_ACTIVE' | translate }}
        </mat-slide-toggle>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">{{ 'COMMON.CANCEL' | translate }}</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="form.invalid || saving">
        {{ (saving ? 'COMMON.SAVING' : 'COMMON.SAVE') | translate }}
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
    private translate: TranslateService,
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
          this.translate.instant('APARTMENTS.SAVE_SUCCESS'),
          this.translate.instant('COMMON.CLOSE'),
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
        this.snackBar.open(message, this.translate.instant('COMMON.CLOSE'), { duration: 5000 });
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
