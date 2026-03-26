import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { SupplierService } from '../../../core/services/supplier.service';
import { SupplierListDto, SERVICE_CATEGORIES } from '../../../core/models/supplier.models';
import { SupplierDialogComponent } from './supplier-dialog/supplier-dialog.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-admin-suppliers-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './admin-suppliers-list.component.html',
  styleUrls: ['./admin-suppliers-list.component.scss']
})
export class AdminSuppliersListComponent implements OnInit {
  displayedColumns: string[] = ['name', 'serviceCategory', 'phone', 'email', 'isActive', 'expenseCount', 'actions'];
  dataSource = new MatTableDataSource<SupplierListDto>();
  serviceCategories = SERVICE_CATEGORIES;
  selectedCategory = '';
  searchText = '';
  isLoading = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private supplierService: SupplierService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadSuppliers();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadSuppliers(): void {
    this.isLoading = true;
    this.supplierService.getAll(this.selectedCategory || undefined, this.searchText || undefined)
      .subscribe({
        next: (suppliers) => {
          this.dataSource.data = suppliers;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Erreur lors du chargement des fournisseurs:', error);
          this.snackBar.open('Erreur lors du chargement des fournisseurs', 'Fermer', { duration: 3000 });
          this.isLoading = false;
        }
      });
  }

  onCategoryChange(): void {
    this.loadSuppliers();
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchText = input.value;
    this.loadSuppliers();
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(SupplierDialogComponent, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadSuppliers();
      }
    });
  }

  openEditDialog(supplier: SupplierListDto): void {
    const dialogRef = this.dialog.open(SupplierDialogComponent, {
      width: '600px',
      data: { mode: 'edit', supplierId: supplier.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadSuppliers();
      }
    });
  }

  deleteSupplier(supplier: SupplierListDto): void {
    if (supplier.expenseCount > 0) {
      this.snackBar.open(
        `Ce fournisseur est utilisé dans ${supplier.expenseCount} dépense(s) et ne peut pas être supprimé`,
        'Fermer',
        { duration: 5000 }
      );
      return;
    }

    if (confirm(`Êtes-vous sûr de vouloir supprimer le fournisseur "${supplier.name}" ?`)) {
      this.supplierService.delete(supplier.id).subscribe({
        next: () => {
          this.snackBar.open('Fournisseur supprimé avec succès', 'Fermer', { duration: 3000 });
          this.loadSuppliers();
        },
        error: (error) => {
          console.error('Erreur lors de la suppression:', error);
          this.snackBar.open(
            error.error?.message || 'Erreur lors de la suppression du fournisseur',
            'Fermer',
            { duration: 5000 }
          );
        }
      });
    }
  }

  clearFilters(): void {
    this.selectedCategory = '';
    this.searchText = '';
    this.loadSuppliers();
  }
}
