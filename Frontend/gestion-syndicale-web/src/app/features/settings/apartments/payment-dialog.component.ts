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
import { PaymentService } from '../../../core/services/payment.service';
import { MonthCheckbox, MONTHS_FR, CreatePaymentDto, ApartmentPaymentStatus } from '../../../core/models/payment.models';
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
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon class="dialog-icon">payments</mat-icon>
      <span *ngIf="data.apartment">Ajouter un paiement – Appartement {{ data.apartment.apartmentNumber }} - {{ data.apartment.buildingNumber }}</span>
      <span *ngIf="!data.apartment">Ajouter une cotisation mensuelle</span>
    </h2>
    
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width" *ngIf="data.mode === 'fromRevenues'">
          <mat-label>Immeuble *</mat-label>
          <mat-select formControlName="buildingId" (selectionChange)="onBuildingChange()">
            <mat-option *ngFor="let building of buildings" [value]="building.id">
              {{ building.buildingNumber }} - {{ building.address }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width" *ngIf="data.mode === 'fromRevenues'">
          <mat-label>Appartement *</mat-label>
          <mat-select formControlName="apartmentId" [disabled]="!form.get('buildingId')?.value">
            <mat-option *ngFor="let apt of filteredApartments" [value]="apt.id">
              Appartement {{ apt.apartmentNumber }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Année *</mat-label>
          <mat-select formControlName="year" (selectionChange)="onYearChange()">
            <mat-option *ngFor="let year of availableYears" [value]="year">
              {{ year }}
            </mat-option>
          </mat-select>
        </mat-form-field>

        <div *ngIf="loadingMonths" class="loading-container">
          <mat-spinner diameter="40"></mat-spinner>
          <p>Chargement des mois payés...</p>
        </div>

        <div *ngIf="!loadingMonths && months.length > 0">
          <div class="select-all-container">
            <mat-checkbox
              [checked]="allMonthsSelected"
              [indeterminate]="someMonthsSelected"
              [disabled]="!canSelectAllMonths"
              (change)="onSelectAllToggle($event.checked)"
              class="select-all-checkbox">
              <strong>Sélectionner toute l'année</strong>
            </mat-checkbox>
          </div>

          <div class="months-grid">
            <div *ngFor="let month of months" class="month-item">
              <mat-checkbox
                [checked]="month.isSelected || month.isPaid"
                [disabled]="month.isDisabled || month.isPaid"
                (change)="onMonthToggle(month, $event.checked)"
                [class.paid-month]="month.isPaid"
                [class.disabled-month]="month.isDisabled && !month.isPaid">
                {{ month.monthName }}
                <span *ngIf="month.isPaid" class="paid-badge">Payé</span>
              </mat-checkbox>
            </div>
          </div>
        </div>

        <div *ngIf="!loadingMonths && paymentStatusMessage" class="info-message">
          <mat-icon>info</mat-icon>
          <span>{{ paymentStatusMessage }}</span>
        </div>

        <div *ngIf="!loadingMonths && selectedMonthsCount > 0" class="summary">
          <mat-icon>info</mat-icon>
          <span>{{ selectedMonthsCount }} mois sélectionné(s) pour le paiement</span>
        </div>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()" [disabled]="saving">Annuler</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onSave()" 
        [disabled]="form.invalid || saving || selectedMonthsCount === 0">
        <mat-icon>save</mat-icon>
        {{ saving ? 'Enregistrement...' : 'Enregistrer le paiement' }}
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
      mat-checkbox {
        width: 100%;
      }
      
      &.paid-month {
        mat-checkbox {
          opacity: 0.7;
        }
      }
    }

    .disabled-month {
      mat-checkbox {
        opacity: 0.5;
      }
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

  get allMonthsSelected(): boolean {
    const selectableMonths = this.months.filter(m => !m.isDisabled && !m.isPaid);
    if (selectableMonths.length === 0) return false;
    return selectableMonths.every(m => m.isSelected);
  }

  get someMonthsSelected(): boolean {
    const selectableMonths = this.months.filter(m => !m.isDisabled && !m.isPaid);
    if (selectableMonths.length === 0) return false;
    const selectedCount = selectableMonths.filter(m => m.isSelected).length;
    return selectedCount > 0 && selectedCount < selectableMonths.length;
  }

  get canSelectAllMonths(): boolean {
    return this.months.some(m => !m.isDisabled && !m.isPaid);
  }

  constructor(
    private fb: FormBuilder,
    private paymentService: PaymentService,
    private snackBar: MatSnackBar,
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
      const monthName = MONTHS_FR[this.paymentStatus.firstUnpaidMonth - 1];
      
      if (selectedYear === this.paymentStatus.firstUnpaidYear) {
        this.paymentStatusMessage = `Premier mois impayé : ${monthName} ${this.paymentStatus.firstUnpaidYear}. Vous devez payer dans l'ordre chronologique.`;
      } else if (selectedYear > this.paymentStatus.firstUnpaidYear) {
        this.paymentStatusMessage = `Vous devez d'abord payer les mois de l'année ${this.paymentStatus.firstUnpaidYear} (à partir de ${monthName}).`;
      } else {
        this.paymentStatusMessage = '';
      }
    } else if (this.paymentStatus.lastPaidYear && this.paymentStatus.lastPaidMonth) {
      const lastMonthName = MONTHS_FR[this.paymentStatus.lastPaidMonth - 1];
      this.paymentStatusMessage = `Tous les paiements sont à jour jusqu'à ${lastMonthName} ${this.paymentStatus.lastPaidYear}.`;
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
        this.snackBar.open('Erreur lors du chargement des immeubles', 'Fermer', { duration: 3000 });
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
    // Vérifier si cette année est l'année du premier impayé
    const isFirstUnpaidYear = this.paymentStatus?.firstUnpaidYear === year;
    const firstUnpaidMonth = this.paymentStatus?.firstUnpaidMonth || 1;

    for (let i = 1; i <= 12; i++) {
      const isPaid = paidMonths.includes(i);

      let isDisabled = false;

      if (isPaid) {
        // Mois déjà payé : désactivé
        isDisabled = true;
      } else if (!isFirstUnpaidYear) {
        // Année différente du premier impayé : tout désactivé
        isDisabled = true;
      } else if (i < firstUnpaidMonth) {
        // Mois avant le premier impayé dans la bonne année : désactivé (ne devrait pas arriver)
        isDisabled = true;
      } else if (i > firstUnpaidMonth) {
        // Mois après le premier impayé : désactivé jusqu'à ce que les précédents soient cochés
        isDisabled = !this.areAllPreviousMonthsChecked(i, paidMonths);
      }
      // Si i === firstUnpaidMonth : enabled (premier mois à payer)
      
      this.months.push({
        monthNumber: i,
        monthName: MONTHS_FR[i - 1],
        isPaid: isPaid,
        isDisabled: isDisabled,
        isSelected: false
      });
    }
  }

  /**
   * Vérifie si tous les mois avant le mois donné sont payés ou cochés
   */
  areAllPreviousMonthsChecked(month: number, paidMonths: number[]): boolean {
    const firstUnpaidMonth = this.paymentStatus?.firstUnpaidMonth || 1;
    
    for (let i = firstUnpaidMonth; i < month; i++) {
      const monthData = this.months.find(m => m.monthNumber === i);
      const isPaid = paidMonths.includes(i);
      const isSelected = monthData?.isSelected || false;
      
      if (!isPaid && !isSelected) {
        return false; // Il y a un mois non payé et non coché avant
      }
    }
    return true;
  }

  onMonthToggle(month: MonthCheckbox, checked: boolean): void {
    if (!month.isPaid && !month.isDisabled) {
      month.isSelected = checked;
      
      // Débloquer le mois suivant si on coche
      if (checked) {
        const nextMonthIndex = this.months.findIndex(m => m.monthNumber === month.monthNumber + 1);
        if (nextMonthIndex !== -1) {
          const nextMonth = this.months[nextMonthIndex];
          if (!nextMonth.isPaid) {
            nextMonth.isDisabled = false;
          }
        }
      } else {
        // Bloquer les mois suivants si on décoche
        for (let i = month.monthNumber + 1; i <= 12; i++) {
          const laterMonth = this.months.find(m => m.monthNumber === i);
          if (laterMonth && !laterMonth.isPaid) {
            laterMonth.isDisabled = true;
            laterMonth.isSelected = false;
          }
        }
      }
    }
  }

  onSelectAllToggle(checked: boolean): void {
    if (checked) {
      // Sélectionner tous les mois disponibles en respectant l'ordre chronologique
      for (const month of this.months) {
        if (!month.isPaid && !month.isDisabled) {
          month.isSelected = true;
          // Débloquer le mois suivant
          const nextMonth = this.months.find(m => m.monthNumber === month.monthNumber + 1);
          if (nextMonth && !nextMonth.isPaid) {
            nextMonth.isDisabled = false;
          }
        }
      }
    } else {
      // Désélectionner tous les mois sélectionnables
      const firstUnpaidMonth = this.paymentStatus?.firstUnpaidMonth || 1;
      for (const month of this.months) {
        if (!month.isPaid) {
          month.isSelected = false;
          // Réactiver les règles de verrouillage
          if (month.monthNumber > firstUnpaidMonth) {
            month.isDisabled = true;
          }
        }
      }
      // Le premier mois impayé reste débloqué
      const firstMonth = this.months.find(m => m.monthNumber === firstUnpaidMonth);
      if (firstMonth && !firstMonth.isPaid) {
        firstMonth.isDisabled = false;
      }
    }
  }

  onSave(): void {
    if (this.form.invalid) return;

    const selectedMonths = this.months
      .filter(m => m.isSelected && !m.isPaid)
      .map(m => m.monthNumber);

    if (selectedMonths.length === 0) {
      this.snackBar.open('Veuillez sélectionner au moins un mois', 'Fermer', { 
        duration: 3000 
      });
      return;
    }

    // Get apartment ID from either data.apartment or form
    let apartmentId: number;
    if (this.data.apartment) {
      apartmentId = this.data.apartment.id;
    } else if (this.data.mode === 'fromRevenues') {
      apartmentId = this.form.get('apartmentId')?.value;
      if (!apartmentId) {
        this.snackBar.open('Veuillez sélectionner un appartement', 'Fermer', { duration: 3000 });
        return;
      }
    } else {
      return;
    }

    this.saving = true;

    const dto: CreatePaymentDto = {
      apartmentId: apartmentId,
      year: this.form.get('year')?.value,
      months: selectedMonths
    };

    this.paymentService.createMonthlyPayment(dto).subscribe({
      next: () => {
        this.snackBar.open(
          `Paiement enregistré avec succès pour ${selectedMonths.length} mois`,
          'Fermer',
          { duration: 3000 }
        );
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error saving payment:', error);
        let message = 'Erreur lors de l\'enregistrement du paiement';
        
        if (error.error?.message) {
          message = error.error.message;
        }
        
        this.snackBar.open(message, 'Fermer', { duration: 5000 });
        this.saving = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
