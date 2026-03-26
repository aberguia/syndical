import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PaymentService } from '../../../core/services/payment.service';
import { MonthCheckbox, CreatePaymentDto, CancelMonthlyPaymentDto, ApartmentPaymentStatus } from '../../../core/models/payment.models';
import { Apartment } from '../../../core/models/settings.models';

@Component({
  selector: 'app-payment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    TranslateModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon class="dialog-icon">payments</mat-icon>
      <span *ngIf="data.apartment">{{ 'PAYMENT.DIALOG_TITLE' | translate: { number: data.apartment.apartmentNumber, building: data.apartment.buildingNumber } }}</span>
      <span *ngIf="!data.apartment">{{ 'PAYMENT.DIALOG_TITLE_REVENUE' | translate }}</span>
    </h2>

    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width" *ngIf="data.mode === 'fromRevenues'">
          <mat-label>{{ 'PAYMENT.FIELD_BUILDING' | translate }}</mat-label>
          <mat-select formControlName="buildingId" (selectionChange)="onBuildingChange()">
            <mat-option *ngFor="let building of buildings" [value]="building.id">
              {{ building.buildingNumber }} - {{ building.address }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width" *ngIf="data.mode === 'fromRevenues'">
          <mat-label>{{ 'PAYMENT.FIELD_APARTMENT' | translate }}</mat-label>
          <mat-select formControlName="apartmentId" [disabled]="!form.get('buildingId')?.value">
            <mat-option *ngFor="let apt of filteredApartments" [value]="apt.id">
              {{ 'COMMON.APARTMENT' | translate }} {{ apt.apartmentNumber }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>{{ 'PAYMENT.FIELD_YEAR' | translate }}</mat-label>
          <mat-select formControlName="year" (selectionChange)="onYearChange()">
            <mat-option *ngFor="let year of availableYears" [value]="year">
              {{ year }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <div *ngIf="loadingMonths" class="loading-container">
          <mat-spinner diameter="40"></mat-spinner>
          <p>{{ 'PAYMENT.LOADING_MONTHS' | translate }}</p>
        </div>

        <div *ngIf="!loadingMonths && months.length > 0">
          <div class="select-all-container">
            <mat-checkbox
              [checked]="allMonthsSelected"
              [indeterminate]="someMonthsSelected"
              [disabled]="!canSelectAllMonths"
              (change)="onSelectAllToggle($event.checked)"
              class="select-all-checkbox">
              <strong>{{ 'PAYMENT.SELECT_ALL' | translate }}</strong>
            </mat-checkbox>
          </div>

          <div class="months-grid">
            <div *ngFor="let month of months" class="month-item">
              <mat-checkbox
                [checked]="(month.isPaid && !month.markedForRemoval) || month.isSelected"
                (change)="onMonthToggle(month, $event.checked)"
                [class.paid-month]="month.isPaid && !month.markedForRemoval"
                [class.removal-month]="month.markedForRemoval"
                [style.opacity]="month.isDisabled ? '0.4' : '1'">
                {{ month.monthName }}
                <span *ngIf="month.isPaid && !month.markedForRemoval" class="paid-badge">{{ 'PAYMENT.PAID_BADGE' | translate }}</span>
                <span *ngIf="month.markedForRemoval" class="removal-badge">{{ 'PAYMENT.REMOVAL_BADGE' | translate }}</span>
              </mat-checkbox>
              <div *ngIf="month.isDisabled" class="month-overlay"></div>
            </div>
          </div>
        </div>

        <div *ngIf="!loadingMonths && paymentStatusMessage && monthsToAddCount === 0 && monthsToRemoveCount === 0" class="info-message">
          <mat-icon>info</mat-icon>
          <span>{{ paymentStatusMessage }}</span>
        </div>

        <div *ngIf="!loadingMonths && (monthsToAddCount > 0 || monthsToRemoveCount > 0)" class="summary">
          <mat-icon>info</mat-icon>
          <span *ngIf="monthsToAddCount > 0">{{ 'PAYMENT.MONTHS_SELECTED' | translate: { count: monthsToAddCount } }}</span>
          <span *ngIf="monthsToAddCount > 0 && monthsToRemoveCount > 0"> — </span>
          <span *ngIf="monthsToRemoveCount > 0">{{ 'PAYMENT.MONTHS_TO_REMOVE' | translate: { count: monthsToRemoveCount } }}</span>
        </div>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="saving">{{ 'COMMON.CANCEL' | translate }}</button>
      <button
        mat-raised-button
        color="primary"
        (click)="onSave()"
        [disabled]="form.invalid || saving || (monthsToAddCount === 0 && monthsToRemoveCount === 0)">
        <mat-icon>save</mat-icon>
        {{ (saving ? 'PAYMENT.SAVING' : 'PAYMENT.SAVE') | translate }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-icon {
      vertical-align: middle;
      margin-right: 8px;
      color: #4caf50;
    }

    .full-width {
      width: 100%;
      margin-bottom: 20px;
    }

    mat-dialog-content {
      min-width: 600px;
      max-width: 700px;
      padding: 20px 24px;
      min-height: 300px;
    }

    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 40px;
      
      mat-spinner {
        margin-bottom: 16px;
      }
      
      p {
        color: #666;
        margin: 0;
      }
    }

    .select-all-container {
      padding: 12px 16px;
      background: #f5f5f5;
      border-radius: 8px;
      margin-bottom: 20px;
      border: 2px solid #e0e0e0;
      
      .select-all-checkbox {
        strong {
          color: #1976d2;
          font-size: 15px;
        }
      }
    }

    .months-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 16px;
      margin-bottom: 20px;
    }

    .month-item {
      position: relative;

      mat-checkbox {
        width: 100%;
      }
    }

    .month-overlay {
      position: absolute;
      inset: 0;
      cursor: not-allowed;
      z-index: 1;
    }

    .paid-badge {
      font-size: 11px;
      color: #4caf50;
      font-weight: 500;
      margin-left: 4px;
      background: #e8f5e9;
      padding: 2px 6px;
      border-radius: 4px;
    }

    .removal-badge {
      font-size: 11px;
      color: #f44336;
      font-weight: 500;
      margin-left: 4px;
      background: #ffebee;
      padding: 2px 6px;
      border-radius: 4px;
    }

    .info-message {
      display: flex;
      align-items: flex-start;
      padding: 12px;
      background: #fff3e0;
      border-left: 4px solid #ff9800;
      border-radius: 4px;
      margin-top: 16px;
      
      mat-icon {
        margin-right: 8px;
        color: #f57c00;
        font-size: 20px;
        width: 20px;
        height: 20px;
        flex-shrink: 0;
      }
      
      span {
        color: #e65100;
        font-size: 13px;
        line-height: 1.4;
      }
    }

    .summary {
      display: flex;
      align-items: center;
      padding: 12px;
      background: #e3f2fd;
      border-radius: 4px;
      margin-top: 16px;
      
      mat-icon {
        margin-right: 8px;
        color: #1976d2;
        font-size: 20px;
        width: 20px;
        height: 20px;
      }
      
      span {
        color: #1565c0;
        font-weight: 500;
      }
    }

    mat-dialog-actions {
      padding: 16px 24px;
      
      button {
        margin-left: 8px;
        
        mat-icon {
          margin-right: 4px;
          font-size: 18px;
          width: 18px;
          height: 18px;
        }
      }
    }
  `]
})
export class PaymentDialogComponent implements OnInit {
  form: FormGroup;
  months: MonthCheckbox[] = [];
  availableYears: number[] = [];
  loadingMonths = false;
  saving = false;
  currentYear: number;
  paymentStatus: ApartmentPaymentStatus | null = null;
  paymentStatusMessage: string = '';
  buildings: any[] = [];
  allApartments: Apartment[] = [];
  filteredApartments: Apartment[] = [];

  get monthsToAddCount(): number {
    return this.months.filter(m => m.isSelected && !m.isPaid).length;
  }

  get monthsToRemoveCount(): number {
    return this.months.filter(m => m.isPaid && m.markedForRemoval).length;
  }

  get allMonthsSelected(): boolean {
    const unpaid = this.months.filter(m => !m.isPaid);
    if (unpaid.length === 0) return false;
    return unpaid.every(m => m.isSelected);
  }

  get someMonthsSelected(): boolean {
    const unpaid = this.months.filter(m => !m.isPaid);
    if (unpaid.length === 0) return false;
    const count = unpaid.filter(m => m.isSelected).length;
    return count > 0 && count < unpaid.length;
  }

  get canSelectAllMonths(): boolean {
    return this.months.some(m => !m.isPaid);
  }

  constructor(
    private fb: FormBuilder,
    private paymentService: PaymentService,
    private snackBar: MatSnackBar,
    private translate: TranslateService,
    public dialogRef: MatDialogRef<PaymentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { apartment?: Apartment; mode?: string }
  ) {
    const now = new Date();
    this.currentYear = now.getFullYear();

    // Générer les années disponibles (2022 -> année courante)
    for (let year = 2022; year <= this.currentYear; year++) {
      this.availableYears.push(year);
    }

    const formConfig: any = {
      year: [this.currentYear, Validators.required]
    };

    if (this.data.mode === 'fromRevenues') {
      formConfig.buildingId = [null, Validators.required];
      formConfig.apartmentId = [null, Validators.required];
    }

    this.form = this.fb.group(formConfig);
  }

  ngOnInit(): void {
    if (this.data.mode === 'fromRevenues') {
      // Load buildings and apartments for selection
      this.loadBuildings();
      
      // Watch for apartment selection to load payment status and paid months
      this.form.get('apartmentId')?.valueChanges.subscribe((apartmentId) => {
        if (apartmentId) {
          this.loadPaymentStatusAndMonths(apartmentId);
        }
      });
    } else if (this.data.apartment) {
      // Load payment status for specific apartment
      this.loadPaymentStatusAndMonths(this.data.apartment.id);
    }
  }

  loadPaymentStatusAndMonths(apartmentId: number): void {
    this.paymentService.getPaymentStatus(apartmentId).subscribe({
      next: (status) => {
        this.paymentStatus = status;
        this.updateStatusMessage();
        this.loadPaidMonths();
      },
      error: (error) => {
        console.error('Error loading payment status:', error);
        this.loadPaidMonths(); // Continuer même en cas d'erreur
      }
    });
  }

  get selectedMonthsCount(): number {
    return this.months.filter(m => m.isSelected && !m.isPaid).length;
  }

  updateStatusMessage(): void {
    if (!this.paymentStatus) {
      this.paymentStatusMessage = '';
      return;
    }

    const selectedYear = this.form.get('year')?.value;

    if (this.paymentStatus.firstUnpaidYear && this.paymentStatus.firstUnpaidMonth) {
      const monthName = this.translate.instant('MONTHS.' + this.paymentStatus.firstUnpaidMonth);

      if (selectedYear === this.paymentStatus.firstUnpaidYear) {
        this.paymentStatusMessage = this.translate.instant('PAYMENT.FIRST_UNPAID', { month: monthName, year: this.paymentStatus.firstUnpaidYear });
      } else if (selectedYear > this.paymentStatus.firstUnpaidYear) {
        this.paymentStatusMessage = this.translate.instant('PAYMENT.PAY_FIRST', { year: this.paymentStatus.firstUnpaidYear, month: monthName });
      } else {
        this.paymentStatusMessage = '';
      }
    } else if (this.paymentStatus.lastPaidYear && this.paymentStatus.lastPaidMonth) {
      const lastMonthName = this.translate.instant('MONTHS.' + this.paymentStatus.lastPaidMonth);
      this.paymentStatusMessage = this.translate.instant('PAYMENT.UP_TO_DATE', { month: lastMonthName, year: this.paymentStatus.lastPaidYear });
    }
  }

  onYearChange(): void {
    this.updateStatusMessage();
    this.loadPaidMonths();
  }

  loadBuildings(): void {
    // Assuming you have a service to load buildings - adjust as needed
    this.paymentService.getBuildings().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
        this.snackBar.open(this.translate.instant('COMMON.ERROR_LOADING'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
      }
    });

    // Load all apartments
    this.paymentService.getApartments().subscribe({
      next: (apartments) => {
        this.allApartments = apartments;
      },
      error: (error) => {
        console.error('Error loading apartments:', error);
      }
    });
  }

  onBuildingChange(): void {
    const buildingId = this.form.get('buildingId')?.value;
    if (buildingId) {
      this.filteredApartments = this.allApartments.filter(apt => apt.buildingId === buildingId);
      this.form.patchValue({ apartmentId: null });
    } else {
      this.filteredApartments = [];
    }
  }

  loadPaidMonths(): void {
    const selectedYear = this.form.get('year')?.value;
    if (!selectedYear) return;

    // Get apartment ID from either data.apartment or form
    let apartmentId: number | undefined;
    if (this.data.apartment) {
      apartmentId = this.data.apartment.id;
    } else if (this.data.mode === 'fromRevenues') {
      apartmentId = this.form.get('apartmentId')?.value;
      if (!apartmentId) return; // Don't load until apartment is selected
    }

    if (!apartmentId) return;

    this.loadingMonths = true;
    this.months = [];

    this.paymentService.getPaidMonthsByApartmentAndYear(apartmentId, selectedYear).subscribe({
      next: (response) => {
        const paidMonths = response.paidMonths;
        this.buildMonthsArray(selectedYear, paidMonths);
        this.loadingMonths = false;
      },
      error: (error) => {
        console.error('Error loading paid months:', error);
        this.buildMonthsArray(selectedYear, []);
        this.loadingMonths = false;
      }
    });
  }

  buildMonthsArray(year: number, paidMonths: number[]): void {
    const months: MonthCheckbox[] = [];
    for (let i = 1; i <= 12; i++) {
      months.push({
        monthNumber: i,
        monthName: this.translate.instant('MONTHS.' + i),
        isPaid: paidMonths.includes(i),
        isDisabled: false,
        isSelected: false,
        markedForRemoval: false
      });
    }
    this.months = this.rebuildDisabledState(months);
  }

  onMonthToggle(month: MonthCheckbox, checked: boolean): void {
    if (month.isDisabled) return;

    let updated: MonthCheckbox[];

    if (checked) {
      updated = this.months.map(m => {
        if (m.monthNumber !== month.monthNumber) return m;
        if (m.isPaid && m.markedForRemoval) return { ...m, markedForRemoval: false };
        if (!m.isPaid) return { ...m, isSelected: true };
        return m;
      });
    } else {
      updated = this.months.map(m => {
        if (m.monthNumber === month.monthNumber) {
          return m.isPaid ? { ...m, markedForRemoval: true } : { ...m, isSelected: false };
        }
        if (m.monthNumber > month.monthNumber) {
          if (m.isPaid && !m.markedForRemoval) return { ...m, markedForRemoval: true };
          if (m.isSelected) return { ...m, isSelected: false };
        }
        return m;
      });
    }

    this.months = this.rebuildDisabledState(updated);
  }

  onSelectAllToggle(checked: boolean): void {
    if (checked) {
      const updated = this.months.map(m =>
        !m.isPaid ? { ...m, isSelected: true } : m
      );
      this.months = this.rebuildDisabledState(updated);
    } else {
      const updated = this.months.map(m =>
        !m.isPaid ? { ...m, isSelected: false } : m
      );
      this.months = this.rebuildDisabledState(updated);
    }
  }

  private rebuildDisabledState(months: MonthCheckbox[]): MonthCheckbox[] {
    // Mois "actifs" = payés (non annulés) OU sélectionnés
    const isActive = (m: MonthCheckbox) =>
      (m.isPaid && !m.markedForRemoval) || m.isSelected;

    const lastActiveNumber = months
      .filter(isActive)
      .reduce((max, m) => Math.max(max, m.monthNumber), 0);

    // Prochain mois non actif après le dernier actif
    const nextToCheck = months.find(
      m => !isActive(m) && m.monthNumber > lastActiveNumber
    )?.monthNumber ?? -1;

    return months.map(m => {
      if (isActive(m)) {
        // Seul le dernier actif est cliquable (pour décocher)
        return { ...m, isDisabled: m.monthNumber !== lastActiveNumber };
      } else {
        // Seul le prochain en séquence est cliquable (pour cocher)
        return { ...m, isDisabled: m.monthNumber !== nextToCheck };
      }
    });
  }

  onSave(): void {
    if (this.form.invalid) return;

    const monthsToAdd = this.months.filter(m => m.isSelected && !m.isPaid).map(m => m.monthNumber);
    const monthsToRemove = this.months.filter(m => m.isPaid && m.markedForRemoval).map(m => m.monthNumber);

    if (monthsToAdd.length === 0 && monthsToRemove.length === 0) {
      this.snackBar.open(this.translate.instant('PAYMENT.NO_MONTH_SELECTED'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
      return;
    }

    let apartmentId: number;
    if (this.data.apartment) {
      apartmentId = this.data.apartment.id;
    } else if (this.data.mode === 'fromRevenues') {
      apartmentId = this.form.get('apartmentId')?.value;
      if (!apartmentId) {
        this.snackBar.open(this.translate.instant('PAYMENT.NO_APARTMENT'), this.translate.instant('COMMON.CLOSE'), { duration: 3000 });
        return;
      }
    } else {
      return;
    }

    this.saving = true;
    const year = this.form.get('year')?.value;

    const addObs = monthsToAdd.length > 0
      ? this.paymentService.createMonthlyPayment({ apartmentId, year, months: monthsToAdd })
      : null;

    const removeObs = monthsToRemove.length > 0
      ? this.paymentService.cancelMonthlyPayments({ apartmentId, year, months: monthsToRemove })
      : null;

    const finish = () => {
      this.snackBar.open(
        this.translate.instant('PAYMENT.SAVE_SUCCESS', { count: monthsToAdd.length + monthsToRemove.length }),
        this.translate.instant('COMMON.CLOSE'),
        { duration: 3000 }
      );
      this.dialogRef.close(true);
    };

    const handleError = (error: any) => {
      console.error('Error saving payment:', error);
      this.snackBar.open(
        error.error?.message || this.translate.instant('PAYMENT.SAVE_ERROR'),
        this.translate.instant('COMMON.CLOSE'),
        { duration: 5000 }
      );
      this.saving = false;
    };

    if (addObs && removeObs) {
      addObs.subscribe({ next: () => removeObs.subscribe({ next: finish, error: handleError }), error: handleError });
    } else if (addObs) {
      addObs.subscribe({ next: finish, error: handleError });
    } else if (removeObs) {
      removeObs.subscribe({ next: finish, error: handleError });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
