import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CarService } from '../../../core/services/car.service';
import { Car, CreateCarDto, UpdateCarDto, CarType, CarBrand, MemberLookup } from '../../../core/models/parking.models';
import { BuildingService } from '../../../core/services/building.service';
import { Building } from '../../../core/models/settings.models';

@Component({
  selector: 'app-add-edit-car-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './add-edit-car-dialog.component.html',
  styleUrls: ['./add-edit-car-dialog.component.scss']
})
export class AddEditCarDialogComponent implements OnInit {
  carForm: FormGroup;
  isEditMode = false;
  buildings: Building[] = [];
  members: MemberLookup[] = [];
  carBrands = [
    { value: CarBrand.Dacia, label: 'Dacia' },
    { value: CarBrand.Renault, label: 'Renault' },
    { value: CarBrand.Peugeot, label: 'Peugeot' },
    { value: CarBrand.Citroen, label: 'Citroën' },
    { value: CarBrand.Hyundai, label: 'Hyundai' },
    { value: CarBrand.Kia, label: 'Kia' },
    { value: CarBrand.Toyota, label: 'Toyota' },
    { value: CarBrand.Volkswagen, label: 'Volkswagen' },
    { value: CarBrand.Mercedes, label: 'Mercedes' },
    { value: CarBrand.BMW, label: 'BMW' },
    { value: CarBrand.Audi, label: 'Audi' },
    { value: CarBrand.Ford, label: 'Ford' },
    { value: CarBrand.Fiat, label: 'Fiat' },
    { value: CarBrand.Nissan, label: 'Nissan' },
    { value: CarBrand.Suzuki, label: 'Suzuki' },
    { value: CarBrand.Opel, label: 'Opel' },
    { value: CarBrand.Seat, label: 'Seat' },
    { value: CarBrand.Skoda, label: 'Skoda' },
    { value: CarBrand.Mazda, label: 'Mazda' },
    { value: CarBrand.Mitsubishi, label: 'Mitsubishi' },
    { value: CarBrand.Autre, label: 'Autre' }
  ];
  carTypes = [
    { value: CarType.Primary, label: 'Principale (Propriétaire)' },
    { value: CarType.Tenant, label: 'Locataire' },
    { value: CarType.Visitor, label: 'Visiteur' }
  ];

  constructor(
    private fb: FormBuilder,
    private carService: CarService,
    private buildingService: BuildingService,
    private dialogRef: MatDialogRef<AddEditCarDialogComponent>,
    private snackBar: MatSnackBar,
    @Inject(MAT_DIALOG_DATA) public data: Car | null
  ) {
    this.isEditMode = !!data;
    
    this.carForm = this.fb.group({
      brand: [data?.brand ?? CarBrand.Dacia, Validators.required],
      platePart1: [data?.platePart1 || '', [Validators.required, Validators.min(1), Validators.max(99999)]],
      platePart2: [data?.platePart2 || '', [Validators.required, Validators.maxLength(1)]],
      platePart3: [data?.platePart3 || '', [Validators.required, Validators.min(1), Validators.max(99)]],
      carType: [data?.carType ?? CarType.Primary, Validators.required],
      buildingId: [data?.buildingId || null, Validators.required],
      memberId: [data?.memberId || null, Validators.required],
      notes: [data?.notes || '']
    });
  }

  ngOnInit(): void {
    this.loadBuildings();
    
    // Si mode édition, charger les membres de l'immeuble sélectionné
    if (this.isEditMode && this.data?.buildingId) {
      this.loadMembers(this.data.buildingId);
    }

    // Surveiller le changement d'immeuble pour recharger les adhérents
    this.carForm.get('buildingId')?.valueChanges.subscribe(buildingId => {
      if (buildingId) {
        this.loadMembers(buildingId);
        // Réinitialiser le membre si l'immeuble change
        if (!this.isEditMode || buildingId !== this.data?.buildingId) {
          this.carForm.patchValue({ memberId: null });
        }
      } else {
        this.members = [];
        this.carForm.patchValue({ memberId: null });
      }
    });
  }

  loadBuildings(): void {
    this.buildingService.getAll().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
        this.snackBar.open('Erreur lors du chargement des immeubles', 'Fermer', { duration: 3000 });
      }
    });
  }

  loadMembers(buildingId: number): void {
    this.carService.getMembersLookup(buildingId).subscribe({
      next: (members) => {
        this.members = members;
      },
      error: (error) => {
        console.error('Error loading members:', error);
        this.snackBar.open('Erreur lors du chargement des adhérents', 'Fermer', { duration: 3000 });
      }
    });
  }

  onSubmit(): void {
    if (this.carForm.invalid) {
      this.carForm.markAllAsTouched();
      return;
    }

    const formValue = this.carForm.value;

    if (this.isEditMode && this.data) {
      const updateDto: UpdateCarDto = {
        brand: formValue.brand,
        platePart1: formValue.platePart1,
        platePart2: formValue.platePart2,
        platePart3: formValue.platePart3,
        carType: formValue.carType,
        memberId: formValue.memberId,
        notes: formValue.notes || null
      };

      this.carService.update(this.data.id, updateDto).subscribe({
        next: () => {
          this.snackBar.open('Voiture modifiée avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error updating car:', error);
          const errorMessage = error.error?.message || error.message || '';
          if (errorMessage.toLowerCase().includes('plaque') || errorMessage.toLowerCase().includes('existe')) {
            this.snackBar.open(errorMessage || 'Cette plaque d\'immatriculation existe déjà', 'Fermer', { duration: 5000 });
          } else {
            this.snackBar.open('Erreur lors de la modification', 'Fermer', { duration: 3000 });
          }
        }
      });
    } else {
      const createDto: CreateCarDto = {
        brand: formValue.brand,
        platePart1: formValue.platePart1,
        platePart2: formValue.platePart2,
        platePart3: formValue.platePart3,
        carType: formValue.carType,
        memberId: formValue.memberId,
        notes: formValue.notes || null
      };

      this.carService.create(createDto).subscribe({
        next: () => {
          this.snackBar.open('Voiture ajoutée avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error creating car:', error);
          const errorMessage = error.error?.message || error.message || '';
          if (errorMessage.toLowerCase().includes('plaque') || errorMessage.toLowerCase().includes('existe')) {
            this.snackBar.open(errorMessage || 'Cette plaque d\'immatriculation existe déjà', 'Fermer', { duration: 5000 });
          } else {
            this.snackBar.open('Erreur lors de l\'ajout', 'Fermer', { duration: 3000 });
          }
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
