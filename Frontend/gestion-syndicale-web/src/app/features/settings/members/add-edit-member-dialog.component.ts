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
import { MemberService } from '../../../core/services/member.service';
import { ApartmentService } from '../../../core/services/apartment.service';
import { BuildingService } from '../../../core/services/building.service';
import { MemberListDto, CreateMemberDto, UpdateMemberDto } from '../../../core/models/member.models';
import { Apartment, Building } from '../../../core/models/settings.models';

@Component({
  selector: 'app-add-edit-member-dialog',
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
    <h2 mat-dialog-title>{{ getTitle() }}</h2>
    
    <mat-dialog-content>
      <form [formGroup]="form">
        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Prénom *</mat-label>
            <input matInput formControlName="firstName" placeholder="Jean" [readonly]="isReadOnly">
            <mat-error *ngIf="form.get('firstName')?.hasError('required')">
              Le prénom est requis
            </mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Nom *</mat-label>
            <input matInput formControlName="lastName" placeholder="Dupont" [readonly]="isReadOnly">
            <mat-error *ngIf="form.get('lastName')?.hasError('required')">
              Le nom est requis
            </mat-error>
          </mat-form-field>
        </div>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Email</mat-label>
          <input matInput type="email" formControlName="email" placeholder="jean.dupont@example.com" [readonly]="isReadOnly">
          <mat-error *ngIf="form.get('email')?.hasError('email')">
            L'email n'est pas valide
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Téléphone *</mat-label>
          <input matInput formControlName="phoneNumber" placeholder="0612345678 ; 0698765432" [readonly]="isReadOnly">
          <mat-hint>Plusieurs numéros séparés par " ; "</mat-hint>
          <mat-error *ngIf="form.get('phoneNumber')?.hasError('required')">
            Le téléphone est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Rôle *</mat-label>
          <mat-select formControlName="role" [disabled]="isReadOnly">
            <mat-option value="Adherent">Adhérent</mat-option>
            <mat-option value="Admin">Administrateur</mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('role')?.hasError('required')">
            Le rôle est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Immeuble</mat-label>
          <mat-select formControlName="buildingId" (selectionChange)="onBuildingChange($event.value)" [disabled]="isReadOnly">
            <mat-option [value]="null">Aucun</mat-option>
            <mat-option *ngFor="let building of buildings" [value]="building.id">
              {{ building.buildingNumber }} - {{ building.name }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Appartement</mat-label>
          <mat-select formControlName="apartmentId" [disabled]="!form.get('buildingId')?.value || isReadOnly">
            <mat-option [value]="null">Aucun</mat-option>
            <mat-option *ngFor="let apartment of filteredApartments" [value]="apartment.id">
              Appartement {{ apartment.apartmentNumber }} - Étage {{ apartment.floor }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-slide-toggle 
          *ngIf="data" 
          formControlName="isActive" 
          color="primary"
          [disabled]="isReadOnly">
          Actif
        </mat-slide-toggle>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">{{ isReadOnly ? 'Fermer' : 'Annuler' }}</button>
      <button 
        *ngIf="!isReadOnly"
        mat-raised-button 
        color="primary" 
        (click)="onSave()" 
        [disabled]="form.invalid || saving">
        {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .form-row {
      display: flex;
      gap: 16px;
      margin-bottom: 0;
    }

    .half-width {
      flex: 1;
      margin-bottom: 16px;
    }

    mat-dialog-content {
      min-width: 550px;
      max-height: 600px;
      padding: 20px 24px;
    }

    mat-slide-toggle {
      margin-top: 8px;
    }
  `]
})
export class AddEditMemberDialogComponent implements OnInit {
  form: FormGroup;
  saving = false;
  buildings: Building[] = [];
  apartments: Apartment[] = [];
  filteredApartments: Apartment[] = [];
  isReadOnly = false;

  constructor(
    private fb: FormBuilder,
    private memberService: MemberService,
    private apartmentService: ApartmentService,
    private buildingService: BuildingService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<AddEditMemberDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    // Détecter si on est en mode lecture seule (ouvert depuis apartments)
    this.isReadOnly = data?.readOnly === true;
    this.form = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.email]],
      phoneNumber: ['', [Validators.required]],
      role: ['Adherent', [Validators.required]],
      buildingId: [null],
      apartmentId: [null],
      isActive: [true]
    });
  }

  getTitle(): string {
    if (this.isReadOnly) {
      return 'Détails de l\'adhérent';
    }
    return this.data ? 'Modifier un adhérent' : 'Ajouter un adhérent';
  }

  ngOnInit(): void {
    this.loadBuildings();
    this.loadApartments();
  }

  loadBuildings(): void {
    this.buildingService.getAll().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
      }
    });
  }

  loadApartments(): void {
    this.apartmentService.getAll().subscribe({
      next: (apartments) => {
        this.apartments = apartments;
        
        // Patcher le formulaire après le chargement des appartements
        if (this.data) {
          this.form.patchValue({
            firstName: this.data.firstName,
            lastName: this.data.lastName,
            email: this.data.email,
            phoneNumber: this.data.phoneNumber,
            role: this.data.role,
            buildingId: this.data.buildingId || null,
            apartmentId: this.data.apartmentId || null,
            isActive: this.data.isActive
          });

          if (this.data.buildingId) {
            this.filterApartmentsByBuilding(this.data.buildingId);
          }
          
          // Désactiver tout le formulaire si en mode lecture seule
          if (this.isReadOnly) {
            this.form.disable();
          }
        }
      },
      error: (error) => {
        console.error('Error loading apartments:', error);
      }
    });
  }

  onBuildingChange(buildingId: number | null): void {
    this.form.get('apartmentId')?.setValue(null);
    if (buildingId) {
      this.filterApartmentsByBuilding(buildingId);
    } else {
      this.filteredApartments = [];
    }
  }

  filterApartmentsByBuilding(buildingId: number): void {
    this.filteredApartments = this.apartments.filter(apt => apt.buildingId === buildingId);
  }

  onSave(): void {
    if (this.form.invalid) return;

    this.saving = true;

    if (this.data) {
      // Modification
      const updateDto: UpdateMemberDto = {
        firstName: this.form.value.firstName,
        lastName: this.form.value.lastName,
        email: this.form.value.email,
        phoneNumber: this.form.value.phoneNumber,
        role: this.form.value.role,
        apartmentId: this.form.value.apartmentId || undefined,
        isActive: this.form.value.isActive
      };

      this.memberService.update(this.data.id, updateDto).subscribe({
        next: () => {
          this.snackBar.open('Adhérent modifié avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error updating member:', error);
          this.snackBar.open(error.error?.message || 'Erreur lors de la modification', 'Fermer', { duration: 5000 });
          this.saving = false;
        }
      });
    } else {
      // Création
      const createDto: CreateMemberDto = {
        firstName: this.form.value.firstName,
        lastName: this.form.value.lastName,
        email: this.form.value.email,
        phoneNumber: this.form.value.phoneNumber,
        role: this.form.value.role,
        apartmentId: this.form.value.apartmentId || undefined
      };

      this.memberService.create(createDto).subscribe({
        next: () => {
          this.snackBar.open('Adhérent créé avec succès. Un email de bienvenue a été envoyé.', 'Fermer', { duration: 5000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error creating member:', error);
          this.snackBar.open(error.error?.message || 'Erreur lors de la création', 'Fermer', { duration: 5000 });
          this.saving = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
