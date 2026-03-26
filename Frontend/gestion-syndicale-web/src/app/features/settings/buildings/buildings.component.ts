import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BuildingService } from '../../../core/services/building.service';
import { PaymentService } from '../../../core/services/payment.service';
import { Building } from '../../../core/models/settings.models';
import { BuildingDialogComponent } from './building-dialog.component';
import { BuildingPaymentsReportDialogComponent } from './building-payments-report-dialog.component';
import { PaymentStatusBarComponent, PaymentStatusData } from '../apartments/payment-status-bar.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-buildings',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    PaymentStatusBarComponent,
    TranslateModule
  ],
  templateUrl: './buildings.component.html',
  styleUrls: ['./buildings.component.scss']
})
export class BuildingsComponent implements OnInit {
  displayedColumns: string[] = ['buildingNumber', 'name', 'floorCount', 'apartmentsCount', 'isActive', 'cotisations', 'actions'];
  dataSource: MatTableDataSource<Building>;
  loading = true;
  paymentStatusMap: Map<number, PaymentStatusData> = new Map();
  currentYear: number;
  currentMonth: number;
  currentDay: number;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private buildingService: BuildingService,
    private paymentService: PaymentService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.dataSource = new MatTableDataSource<Building>([]);
    
    const now = new Date();
    this.currentYear = now.getFullYear();
    this.currentMonth = now.getMonth() + 1; // 1-12
    this.currentDay = now.getDate();
  }

  ngOnInit(): void {
    this.loadBuildings();
  }

  loadBuildings(): void {
    this.loading = true;
    this.buildingService.getAll().subscribe({
      next: (buildings) => {
        this.dataSource.data = buildings;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.loadPaymentStatus(buildings);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
        this.snackBar.open('Erreur lors du chargement des immeubles', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  loadPaymentStatus(buildings: Building[]): void {
    if (buildings.length === 0) return;

    // UN SEUL appel API pour tous les immeubles
    this.paymentService.getBuildingsPaymentsSummaryByYear(this.currentYear).subscribe({
      next: (summary) => {
        // Créer une map pour accès rapide
        const summaryMap = new Map(
          summary.buildings.map(b => [b.buildingId, { apartmentsCount: b.apartmentsCount, totalPaidMonths: b.totalPaidMonths }])
        );

        // Calculer le statut pour chaque immeuble
        buildings.forEach(building => {
          const data = summaryMap.get(building.id);
          const apartmentsCount = data?.apartmentsCount || 0;
          const totalPaidMonths = data?.totalPaidMonths || 0;
          const statusData = this.calculateBuildingPaymentStatus(apartmentsCount, totalPaidMonths);
          this.paymentStatusMap.set(building.id, statusData);
        });
      },
      error: (error) => {
        console.error('Error loading payment status:', error);
      }
    });
  }

  calculateBuildingPaymentStatus(apartmentsCount: number, totalPaidMonths: number): PaymentStatusData {
    if (apartmentsCount === 0) {
      return { greenMonths: 0, redMonths: 0, blueMonths: 12 };
    }

    // Capacité totale
    const totalCapacity = apartmentsCount * 12;

    // Calcul des mois échus selon la règle du 15
    let dueMonths = 0;
    if (this.currentDay > 15) {
      dueMonths = this.currentMonth;
    } else {
      dueMonths = this.currentMonth - 1;
    }
    dueMonths = Math.max(0, dueMonths);

    // Capacité échue
    const dueCapacity = apartmentsCount * dueMonths;

    // Calcul des segments en capacité
    const greenCapacity = Math.min(totalPaidMonths, totalCapacity);
    const redCapacity = Math.max(0, dueCapacity - greenCapacity);
    const blueCapacity = Math.max(0, totalCapacity - dueCapacity);

    // Conversion en "mois virtuels sur 12" pour le composant
    // Le composant s'attend à des valeurs sur 12 qu'il convertira en %
    const greenMonths = (greenCapacity / totalCapacity) * 12;
    const redMonths = (redCapacity / totalCapacity) * 12;
    const blueMonths = (blueCapacity / totalCapacity) * 12;

    return {
      greenMonths: Math.round(greenMonths * 10) / 10, // Arrondi à 1 décimale
      redMonths: Math.round(redMonths * 10) / 10,
      blueMonths: Math.round(blueMonths * 10) / 10
    };
  }

  getPaymentStatus(buildingId: number): PaymentStatusData {
    return this.paymentStatusMap.get(buildingId) || {
      greenMonths: 0,
      redMonths: 0,
      blueMonths: 12
    };
  }

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(BuildingDialogComponent, {
      width: '500px',
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadBuildings();
      }
    });
  }

  openEditDialog(building: Building): void {
    const dialogRef = this.dialog.open(BuildingDialogComponent, {
      width: '500px',
      data: building
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadBuildings();
      }
    });
  }

  viewApartments(building: Building): void {
    this.router.navigate(['/settings/apartments'], {
      queryParams: { buildingId: building.id }
    });
  }

  openPaymentsReport(building: Building): void {
    const dialogRef = this.dialog.open(BuildingPaymentsReportDialogComponent, {
      width: '500px',
      data: { building }
    });

    dialogRef.afterClosed().subscribe(success => {
      if (success) {
        // PDF généré avec succès
      }
    });
  }

  deleteBuilding(building: Building): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer l'immeuble "${building.name}" ?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.buildingService.delete(building.id).subscribe({
          next: () => {
            this.snackBar.open('Immeuble supprimé avec succès', 'Fermer', { duration: 3000 });
            this.loadBuildings();
          },
          error: (error) => {
            console.error('Error deleting building:', error);
            const message = error.error?.message || 'Impossible de supprimer : l\'immeuble contient des appartements';
            this.snackBar.open(message, 'Fermer', { duration: 5000 });
          }
        });
      }
    });
  }
}
