import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SupplierService } from '../../../../core/services/supplier.service';
import { SERVICE_CATEGORIES, CreateSupplierDto, UpdateSupplierDto } from '../../../../core/models/supplier.models';

@Component({
  selector: 'app-supplier-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatSnackBarModule
  ],
  templateUrl: './supplier-dialog.component.html',
  styleUrls: ['./supplier-dialog.component.scss']
})
export class SupplierDialogComponent implements OnInit {
  form: FormGroup;
  serviceCategories = SERVICE_CATEGORIES;
  isEditMode = false;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private supplierService: SupplierService,
    private snackBar: MatSnackBar,
    private dialogRef: MatDialogRef<SupplierDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: 'create' | 'edit', supplierId?: number }
  ) {
    this.isEditMode = data.mode === 'edit';
    
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      serviceCategory: ['', Validators.required],
      description: [''],
      phone: [''],
      email: ['', Validators.email],
      address: [''],
      isActive: [true]
    });

    if (!this.isEditMode) {
      this.form.removeControl('isActive');
    }
  }

  ngOnInit(): void {
    if (this.isEditMode && this.data.supplierId) {
      this.loadSupplier(this.data.supplierId);
    }
  }

  loadSupplier(id: number): void {
    this.isLoading = true;
    this.supplierService.getById(id).subscribe({
      next: (supplier) => {
        this.form.patchValue({
          name: supplier.name,
          serviceCategory: supplier.serviceCategory,
          description: supplier.description,
          phone: supplier.phone,
          email: supplier.email,
          address: supplier.address,
          isActive: supplier.isActive
        });
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erreur lors du chargement du fournisseur:', error);
        this.snackBar.open('Erreur lors du chargement du fournisseur', 'Fermer', { duration: 3000 });
        this.isLoading = false;
        this.dialogRef.close();
      }
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const formValue = this.form.value;

    if (this.isEditMode && this.data.supplierId) {
      const dto: UpdateSupplierDto = {
        name: formValue.name,
        serviceCategory: formValue.serviceCategory,
        description: formValue.description || undefined,
        phone: formValue.phone || undefined,
        email: formValue.email || undefined,
        address: formValue.address || undefined,
        isActive: formValue.isActive
      };

      this.supplierService.update(this.data.supplierId, dto).subscribe({
        next: () => {
          this.snackBar.open('Fournisseur modifié avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Erreur lors de la modification:', error);
          this.snackBar.open(
            error.error?.message || 'Erreur lors de la modification du fournisseur',
            'Fermer',
            { duration: 5000 }
          );
          this.isLoading = false;
        }
      });
    } else {
      const dto: CreateSupplierDto = {
        name: formValue.name,
        serviceCategory: formValue.serviceCategory,
        description: formValue.description || undefined,
        phone: formValue.phone || undefined,
        email: formValue.email || undefined,
        address: formValue.address || undefined
      };

      this.supplierService.create(dto).subscribe({
        next: () => {
          this.snackBar.open('Fournisseur créé avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Erreur lors de la création:', error);
          this.snackBar.open(
            error.error?.message || 'Erreur lors de la création du fournisseur',
            'Fermer',
            { duration: 5000 }
          );
          this.isLoading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  getErrorMessage(fieldName: string): string {
    const field = this.form.get(fieldName);
    if (field?.hasError('required')) {
      return 'Ce champ est requis';
    }
    if (field?.hasError('minlength')) {
      return 'Minimum 2 caractères';
    }
    if (field?.hasError('email')) {
      return 'Email invalide';
    }
    return '';
  }
}
