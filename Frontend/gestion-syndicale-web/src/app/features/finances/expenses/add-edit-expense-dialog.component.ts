import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ExpensesService } from '../../../core/services/expenses.service';
import { SupplierService } from '../../../core/services/supplier.service';
import { ExpenseDetail, CreateExpenseDto, UpdateExpenseDto } from '../../../core/models/expenses.models';
import { SupplierLookupDto } from '../../../core/models/supplier.models';

@Component({
  selector: 'app-add-edit-expense-dialog',
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
    MatDatepickerModule,
    MatNativeDateModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './add-edit-expense-dialog.component.html',
  styleUrl: './add-edit-expense-dialog.component.scss'
})
export class AddEditExpenseDialogComponent implements OnInit {
  expenseForm: FormGroup;
  loading = false;
  isEditMode = false;
  suppliers: SupplierLookupDto[] = [];
  selectedFiles: File[] = [];
  maxFileSize = 5 * 1024 * 1024; // 5MB
  allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf'];

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddEditExpenseDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { expenseId?: number },
    private expensesService: ExpensesService,
    private supplierService: SupplierService,
    private snackBar: MatSnackBar
  ) {
    this.isEditMode = !!data.expenseId;

    this.expenseForm = this.fb.group({
      expenseDate: [new Date(), Validators.required],
      supplierId: [null],
      amount: [null, [Validators.required, Validators.min(0.01)]],
      description: ['', [Validators.required, Validators.maxLength(500)]],
      invoiceNumber: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.loadSuppliers();
    if (this.isEditMode && this.data.expenseId) {
      this.loadExpense();
    }
  }

  loadSuppliers(): void {
    this.supplierService.getLookups().subscribe({
      next: (suppliers) => {
        this.suppliers = suppliers;
      },
      error: (error) => {
        console.error('Error loading suppliers:', error);
        this.snackBar.open('Erreur lors du chargement des fournisseurs', 'Fermer', { duration: 3000 });
      }
    });
  }

  loadExpense(): void {
    this.loading = true;
    this.expensesService.getExpenseById(this.data.expenseId!).subscribe({
      next: (expense) => {
        this.expenseForm.patchValue({
          expenseDate: new Date(expense.expenseDate),
          supplierId: expense.supplierId,
          amount: expense.amount,
          description: expense.description,
          invoiceNumber: expense.invoiceNumber,
          notes: expense.notes
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading expense:', error);
        this.snackBar.open('Erreur lors du chargement', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.expenseForm.valid) {
      this.loading = true;

      const formValue = this.expenseForm.value;
      const dto: CreateExpenseDto | UpdateExpenseDto = {
        expenseDate: formValue.expenseDate,
        supplierId: formValue.supplierId || undefined,
        amount: formValue.amount,
        description: formValue.description,
        invoiceNumber: formValue.invoiceNumber || undefined,
        notes: formValue.notes || undefined
      };

      const request = this.isEditMode
        ? this.expensesService.update(this.data.expenseId!, dto as UpdateExpenseDto)
        : this.expensesService.create(dto as CreateExpenseDto);

      request.subscribe({
        next: (response: ExpenseDetail) => {
          // Si c'est une création et qu'il y a des fichiers, on les upload
          if (!this.isEditMode && this.selectedFiles.length > 0 && response?.id) {
            this.uploadFiles(response.id);
          } else {
            this.snackBar.open(
              this.isEditMode ? 'Dépense modifiée avec succès' : 'Dépense créée avec succès',
              'Fermer',
              { duration: 3000 }
            );
            this.dialogRef.close(true);
          }
        },
        error: (error) => {
          console.error('Error saving expense:', error);
          const message = error.error?.message || 'Erreur lors de l\'enregistrement';
          this.snackBar.open(message, 'Fermer', { duration: 5000 });
          this.loading = false;
        }
      });
    }
  }

  private uploadFiles(expenseId: number): void {
    let uploadedCount = 0;
    let errors = 0;

    this.selectedFiles.forEach(file => {
      this.expensesService.uploadAttachment(expenseId, file).subscribe({
        next: () => {
          uploadedCount++;
          if (uploadedCount + errors === this.selectedFiles.length) {
            this.handleUploadComplete(uploadedCount, errors);
          }
        },
        error: () => {
          errors++;
          if (uploadedCount + errors === this.selectedFiles.length) {
            this.handleUploadComplete(uploadedCount, errors);
          }
        }
      });
    });
  }

  private handleUploadComplete(uploaded: number, errors: number): void {
    if (errors === 0) {
      this.snackBar.open(`Dépense créée avec ${uploaded} pièce(s) jointe(s)`, 'Fermer', { duration: 3000 });
    } else {
      this.snackBar.open(`Dépense créée. ${uploaded} fichier(s) uploadé(s), ${errors} erreur(s)`, 'Fermer', { duration: 5000 });
    }
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      
      // Validation
      if (!this.allowedTypes.includes(file.type)) {
        this.snackBar.open('Type de fichier non autorisé. Formats acceptés: JPG, PNG, PDF', 'Fermer', { duration: 3000 });
        continue;
      }
      
      if (file.size > this.maxFileSize) {
        this.snackBar.open('Fichier trop volumineux. Taille maximum: 5MB', 'Fermer', { duration: 3000 });
        continue;
      }
      
      this.selectedFiles.push(file);
    }
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  getFileIcon(file: File): string {
    return file.type === 'application/pdf' ? 'picture_as_pdf' : 'image';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }

  getSelectedSupplierCategory(): string {
    const supplierId = this.expenseForm.get('supplierId')?.value;
    if (!supplierId) return '';
    const supplier = this.suppliers.find(s => s.id === supplierId);
    return supplier?.serviceCategory || '';
  }
}
