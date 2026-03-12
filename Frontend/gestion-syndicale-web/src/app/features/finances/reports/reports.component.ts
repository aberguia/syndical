import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ReportsService } from '../../../core/services/reports.service';
import { FinancialSummary } from '../../../core/models/reports.models';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  form: FormGroup;
  loading = false;
  summary: FinancialSummary | null = null;
  residenceStartYear = 2022;
  Math = Math; // Expose Math for template

  // Table columns
  monthlyColumns = ['month', 'contributions', 'otherRevenues', 'totalRevenues', 'expenses', 'netResult'];
  expensesColumns = ['categoryName', 'amount', 'percent'];
  revenuesColumns = ['title', 'amount', 'percent'];

  constructor(
    private fb: FormBuilder,
    private reportsService: ReportsService,
    private snackBar: MatSnackBar
  ) {
    const currentYear = new Date().getFullYear();
    this.form = this.fb.group({
      fromDate: [new Date(currentYear, 0, 1), Validators.required],
      toDate: [new Date(currentYear, 11, 31), Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadSummary();
  }

  setCurrentYear(): void {
    const currentYear = new Date().getFullYear();
    this.form.patchValue({
      fromDate: new Date(currentYear, 0, 1),
      toDate: new Date(currentYear, 11, 31)
    });
    this.loadSummary();
  }

  setCurrentMonth(): void {
    const now = new Date();
    this.form.patchValue({
      fromDate: new Date(now.getFullYear(), now.getMonth(), 1),
      toDate: new Date(now.getFullYear(), now.getMonth() + 1, 0)
    });
    this.loadSummary();
  }

  setQuarter(quarter: number): void {
    const currentYear = new Date().getFullYear();
    const startMonth = (quarter - 1) * 3;
    const endMonth = startMonth + 2;
    
    this.form.patchValue({
      fromDate: new Date(currentYear, startMonth, 1),
      toDate: new Date(currentYear, endMonth + 1, 0)
    });
    this.loadSummary();
  }

  loadSummary(): void {
    if (this.form.invalid) {
      this.snackBar.open('Veuillez sélectionner des dates valides', 'Fermer', { duration: 3000 });
      return;
    }

    const fromDate: Date = this.form.get('fromDate')?.value;
    const toDate: Date = this.form.get('toDate')?.value;

    if (fromDate > toDate) {
      this.snackBar.open('La date de début doit être avant la date de fin', 'Fermer', { duration: 3000 });
      return;
    }

    const from = this.formatDate(fromDate);
    const to = this.formatDate(toDate);

    this.loading = true;
    this.reportsService.getFinancialSummary(from, to).subscribe({
      next: (summary) => {
        this.summary = summary;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading financial summary:', error);
        this.snackBar.open('Erreur lors du chargement du bilan', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  exportPdf(): void {
    if (!this.summary) return;

    const fromDate: Date = this.form.get('fromDate')?.value;
    const toDate: Date = this.form.get('toDate')?.value;
    const from = this.formatDate(fromDate);
    const to = this.formatDate(toDate);

    this.loading = true;
    this.reportsService.downloadFinancialSummaryPdf(from, to).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `bilan_${from}_${to}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.loading = false;
        this.snackBar.open('PDF téléchargé avec succès', 'Fermer', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error downloading PDF:', error);
        this.snackBar.open('Erreur lors de la génération du PDF', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('fr-MA', {
      style: 'currency',
      currency: 'MAD',
      minimumFractionDigits: 2
    }).format(amount);
  }

  formatPercent(value: number): string {
    return `${value.toFixed(1)}%`;
  }

  formatMonth(month: string): string {
    const [year, monthNum] = month.split('-');
    const monthNames = [
      'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin',
      'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'
    ];
    return `${monthNames[parseInt(monthNum) - 1]} ${year}`;
  }

  getResultClass(amount: number): string {
    return amount >= 0 ? 'positive-result' : 'negative-result';
  }

  getResultLabel(amount: number): string {
    return amount >= 0 ? 'Excédent' : 'Déficit';
  }
}
