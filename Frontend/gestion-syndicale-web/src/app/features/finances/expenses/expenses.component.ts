import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatBadgeModule } from '@angular/material/badge';
import { ExpensesService } from '../../../core/services/expenses.service';
import { AuthService } from '../../../core/services/auth.service';
import { Expense } from '../../../core/models/expenses.models';
import { AddEditExpenseDialogComponent } from './add-edit-expense-dialog.component';
import { AttachmentsDialogComponent } from './attachments-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatTooltipModule,
    MatChipsModule,
    MatBadgeModule
  ],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss'
})
export class ExpensesComponent implements OnInit {
  displayedColumns: string[] = ['expenseDate', 'categoryName', 'amount', 'description', 'invoiceNumber', 'supplierName', 'attachmentsCount', 'actions'];
  dataSource: MatTableDataSource<Expense>;
  years: number[] = [];
  months = [
    { value: 0, label: 'Tous les mois' },
    { value: 1, label: 'Janvier' },
    { value: 2, label: 'Février' },
    { value: 3, label: 'Mars' },
    { value: 4, label: 'Avril' },
    { value: 5, label: 'Mai' },
    { value: 6, label: 'Juin' },
    { value: 7, label: 'Juillet' },
    { value: 8, label: 'Août' },
    { value: 9, label: 'Septembre' },
    { value: 10, label: 'Octobre' },
    { value: 11, label: 'Novembre' },
    { value: 12, label: 'Décembre' }
  ];

  yearControl = new FormControl(new Date().getFullYear());
  monthControl = new FormControl(0);
  searchControl = new FormControl('');

  loading = true;
  totalCount = 0;
  totalAmount = 0;
  page = 1;
  pageSize = 25;
  isSuperAdmin = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private expensesService: ExpensesService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.dataSource = new MatTableDataSource<Expense>([]);
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');

    // Générer années (3 dernières + actuelle + 1 future)
    const currentYear = new Date().getFullYear();
    for (let i = currentYear - 3; i <= currentYear + 1; i++) {
      this.years.push(i);
    }
  }

  ngOnInit(): void {
    this.loadExpenses();

    this.yearControl.valueChanges.subscribe(() => this.loadExpenses());
    this.monthControl.valueChanges.subscribe(() => this.loadExpenses());
    
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.page = 1;
      this.loadExpenses();
    });
  }

  loadExpenses(): void {
    this.loading = true;
    const year = this.yearControl.value || new Date().getFullYear();
    const month = this.monthControl.value || undefined;
    const categoryId = undefined; // Categories deprecated - now using suppliers
    const search = this.searchControl.value || undefined;

    this.expensesService.getExpenses(year, month === 0 ? undefined : month, categoryId === 0 ? undefined : categoryId, search, this.page, this.pageSize).subscribe({
      next: (result) => {
        this.dataSource.data = result.items;
        this.totalCount = result.totalCount;
        this.totalAmount = result.totalAmount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading expenses:', error);
        this.snackBar.open('Erreur lors du chargement des dépenses', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadExpenses();
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(AddEditExpenseDialogComponent, {
      width: '700px',
      data: {}
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadExpenses();
      }
    });
  }

  openEditDialog(expense: Expense): void {
    const dialogRef = this.dialog.open(AddEditExpenseDialogComponent, {
      width: '700px',
      data: { expenseId: expense.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadExpenses();
      }
    });
  }

  openAttachmentsDialog(expense: Expense): void {
    this.dialog.open(AttachmentsDialogComponent, {
      width: '800px',
      data: { expenseId: expense.id, expenseDescription: expense.description }
    });
  }

  deleteExpense(expense: Expense): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer cette dépense ?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.expensesService.delete(expense.id).subscribe({
          next: () => {
            this.snackBar.open('Dépense supprimée avec succès', 'Fermer', { duration: 3000 });
            this.loadExpenses();
          },
          error: (error) => {
            console.error('Error deleting expense:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }

  getTruncatedDescription(description: string): string {
    return description.length > 100 ? description.substring(0, 100) + '...' : description;
  }
}
