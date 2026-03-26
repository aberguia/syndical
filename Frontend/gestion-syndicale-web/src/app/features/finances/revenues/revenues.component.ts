import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { RevenuesService } from '../../../core/services/revenues.service';
import { AuthService } from '../../../core/services/auth.service';
import { 
  ContributionsMatrix, 
  OtherRevenue, 
  RevenuesTotal 
} from '../../../core/models/revenues.models';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { TranslateModule } from '@ngx-translate/core';
import { AddEditOtherRevenueDialogComponent } from './add-edit-other-revenue-dialog.component';
import { RevenueAttachmentsDialogComponent } from './revenue-attachments-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { PaymentDialogComponent } from '../../settings/apartments/payment-dialog.component';

@Component({
  selector: 'app-revenues',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatTooltipModule,
    MatBadgeModule,
    TranslateModule
  ],
  templateUrl: './revenues.component.html',
  styleUrl: './revenues.component.scss'
})
export class RevenuesComponent implements OnInit {
  currentYear = new Date().getFullYear();
  loading = false;
  isSuperAdmin = false;

  // Totaux globaux
  totals: RevenuesTotal | null = null;

  // Matrice cotisations
  contributionsMatrix: ContributionsMatrix | null = null;
  monthNames = ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Jun', 'Jul', 'Aoû', 'Sep', 'Oct', 'Nov', 'Déc'];

  // Autres revenus
  otherRevenues: OtherRevenue[] = [];
  totalOtherRevenuesCount = 0;
  totalOtherRevenuesAmount = 0;
  pageSize = 25;
  pageIndex = 0;
  searchControl = new FormControl('');
  
  displayedColumns: string[] = ['revenueDate', 'title', 'amount', 'description', 'attachmentsCount', 'actions'];

  constructor(
    private revenuesService: RevenuesService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    
    this.loadTotals();
    this.loadContributionsMatrix();
    this.loadOtherRevenues();

    // Recherche avec debounce
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(() => {
        this.pageIndex = 0;
        this.loadOtherRevenues();
      });
  }

  loadTotals(): void {
    this.revenuesService.getRevenuesTotals(this.currentYear).subscribe({
      next: (totals) => {
        this.totals = totals;
      },
      error: (error) => {
        console.error('Error loading totals:', error);
      }
    });
  }

  loadContributionsMatrix(): void {
    this.loading = true;
    this.revenuesService.getContributionsMatrix(this.currentYear).subscribe({
      next: (matrix) => {
        this.contributionsMatrix = matrix;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading contributions matrix:', error);
        this.snackBar.open('Erreur lors du chargement de la matrice', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  loadOtherRevenues(): void {
    this.loading = true;
    const search = this.searchControl.value || undefined;
    
    this.revenuesService.getOtherRevenues(
      this.currentYear,
      search,
      this.pageIndex + 1,
      this.pageSize
    ).subscribe({
      next: (result) => {
        this.otherRevenues = result.items;
        this.totalOtherRevenuesCount = result.totalCount;
        this.totalOtherRevenuesAmount = result.totalAmount;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading other revenues:', error);
        this.snackBar.open('Erreur lors du chargement des autres revenus', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadOtherRevenues();
  }

  openAddContributionDialog(): void {
    const dialogRef = this.dialog.open(PaymentDialogComponent, {
      width: '600px',
      data: { mode: 'fromRevenues' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTotals();
        this.loadContributionsMatrix();
      }
    });
  }

  openAddOtherRevenueDialog(): void {
    const dialogRef = this.dialog.open(AddEditOtherRevenueDialogComponent, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTotals();
        this.loadOtherRevenues();
      }
    });
  }

  openEditOtherRevenueDialog(revenue: OtherRevenue): void {
    const dialogRef = this.dialog.open(AddEditOtherRevenueDialogComponent, {
      width: '600px',
      data: { mode: 'edit', revenueId: revenue.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadTotals();
        this.loadOtherRevenues();
      }
    });
  }

  openAttachmentsDialog(revenue: OtherRevenue): void {
    const dialogRef = this.dialog.open(RevenueAttachmentsDialogComponent, {
      width: '800px',
      data: { revenueId: revenue.id, revenueTitle: revenue.title }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadOtherRevenues();
      }
    });
  }

  deleteOtherRevenue(revenue: OtherRevenue): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer le revenu "${revenue.title}" ?`,
        confirmText: 'Supprimer',
        cancelText: 'Annuler'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.revenuesService.deleteOtherRevenue(revenue.id).subscribe({
          next: () => {
            this.snackBar.open('Revenu supprimé avec succès', 'Fermer', { duration: 3000 });
            this.loadTotals();
            this.loadOtherRevenues();
          },
          error: (error) => {
            console.error('Error deleting revenue:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }
}
