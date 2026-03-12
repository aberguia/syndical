import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApartmentService } from '../../../core/services/apartment.service';
import { BuildingService } from '../../../core/services/building.service';
import { AuthService } from '../../../core/services/auth.service';
import { PaymentService } from '../../../core/services/payment.service';
import { Apartment, Building } from '../../../core/models/settings.models';
import { ApartmentDialogComponent } from './apartment-dialog.component';
import { PaymentDialogComponent } from './payment-dialog.component';
import { GenerateReceiptDialogComponent } from './generate-receipt-dialog.component';
import { PaymentStatusBarComponent, PaymentStatusData } from './payment-status-bar.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { AddEditMemberDialogComponent } from '../members/add-edit-member-dialog.component';
import { MemberService } from '../../../core/services/member.service';

@Component({
  selector: 'app-apartments',
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
    MatChipsModule,
    MatTooltipModule,
    PaymentStatusBarComponent
  ],
  templateUrl: './apartments.component.html',
  styleUrls: ['./apartments.component.scss']
})
export class ApartmentsComponent implements OnInit {
  displayedColumns: string[] = ['buildingNumber', 'apartmentNumber', 'floor', 'adherent', 'isActive', 'cotisations', 'actions'];
  dataSource: MatTableDataSource<Apartment>;
  buildings: Building[] = [];
  buildingFilter = new FormControl(0);
  loading = true;
  isSuperAdminOrAdmin = false;
  paymentStatusMap: Map<number, PaymentStatusData> = new Map();
  currentYear: number;
  currentMonth: number;
  currentDay: number;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private apartmentService: ApartmentService,
    private buildingService: BuildingService,
    private authService: AuthService,
    private paymentService: PaymentService,
    private memberService: MemberService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.dataSource = new MatTableDataSource<Apartment>([]);
    this.isSuperAdminOrAdmin = this.authService.hasRole('SuperAdmin') || this.authService.hasRole('Admin');
    
    const now = new Date();
    this.currentYear = now.getFullYear();
    this.currentMonth = now.getMonth() + 1; // 1-12
    this.currentDay = now.getDate();
  }

  ngOnInit(): void {
    this.loadBuildings();
    
    // Recharger les données à chaque retour sur la page
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      if (event.url.includes('/settings/apartments')) {
        const currentBuildingId = this.buildingFilter.value;
        this.loadApartments(currentBuildingId || undefined);
      }
    });
    
    // Écouter les changements de queryParams
    this.route.queryParams.subscribe(params => {
      const buildingId = params['buildingId'];
      if (buildingId) {
        const id = parseInt(buildingId, 10);
        // Attendre que les buildings soient chargés pour appliquer le filtre
        this.applyBuildingFilter(id);
      } else {
        this.loadApartments();
      }
    });
    
    this.buildingFilter.valueChanges.subscribe(buildingId => {
      // Mettre à jour l'URL quand l'utilisateur change le filtre
      if (buildingId && buildingId !== 0) {
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { buildingId },
          queryParamsHandling: 'merge'
        });
      } else {
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: {}
        });
      }
      this.loadApartments(buildingId || undefined);
    });
  }

  loadBuildings(): void {
    this.buildingService.getAll().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
        // Appliquer le filtre depuis l'URL si présent
        const buildingId = this.route.snapshot.queryParams['buildingId'];
        if (buildingId) {
          this.applyBuildingFilter(parseInt(buildingId, 10));
        }
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
      }
    });
  }

  private applyBuildingFilter(buildingId: number): void {
    // Vérifier que le building existe
    const buildingExists = this.buildings.find(b => b.id === buildingId);
    if (buildingExists) {
      // Définir la valeur du FormControl sans émettre d'événement pour éviter la boucle
      this.buildingFilter.setValue(buildingId, { emitEvent: false });
      this.loadApartments(buildingId);
    } else if (this.buildings.length > 0) {
      // Si le building n'existe pas, fallback vers "Tous les immeubles"
      this.buildingFilter.setValue(0, { emitEvent: false });
      this.loadApartments();
    }
  }

  loadApartments(buildingId?: number): void {
    this.loading = true;
    this.apartmentService.getAll(buildingId).subscribe({
      next: (apartments) => {
        this.dataSource.data = apartments;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.loadPaymentStatus(apartments);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading apartments:', error);
        this.snackBar.open('Erreur lors du chargement des appartements', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  loadPaymentStatus(apartments: Apartment[]): void {
    if (apartments.length === 0) return;

    // UN SEUL appel API au lieu de N appels
    this.paymentService.getPaymentsSummaryByYear(this.currentYear).subscribe({
      next: (summary) => {
        // Créer une map pour accès rapide
        const summaryMap = new Map(
          summary.apartments.map(a => [a.apartmentId, a.paidMonthsCount])
        );

        // Calculer le statut pour chaque appartement
        apartments.forEach(apt => {
          const paidMonthsCount = summaryMap.get(apt.id) || 0;
          const statusData = this.calculatePaymentStatus(paidMonthsCount);
          this.paymentStatusMap.set(apt.id, statusData);
        });
      },
      error: (error) => {
        console.error('Error loading payment status:', error);
      }
    });
  }

  calculatePaymentStatus(paidMonthsCount: number): PaymentStatusData {
    // Calcul des mois échus
    let dueMonthsCount = 0;
    if (this.currentDay > 15) {
      dueMonthsCount = this.currentMonth;
    } else {
      dueMonthsCount = this.currentMonth - 1;
    }
    dueMonthsCount = Math.max(0, dueMonthsCount);

    // Calcul des segments
    const greenMonths = paidMonthsCount;
    const redMonths = Math.max(0, dueMonthsCount - paidMonthsCount);
    const blueMonths = 12 - dueMonthsCount;

    return {
      greenMonths,
      redMonths,
      blueMonths
    };
  }

  getPaymentStatus(apartmentId: number): PaymentStatusData {
    return this.paymentStatusMap.get(apartmentId) || {
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
    const dialogRef = this.dialog.open(ApartmentDialogComponent, {
      width: '600px',
      data: { apartment: null, buildings: this.buildings }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadApartments(this.buildingFilter.value || undefined);
      }
    });
  }

  openEditDialog(apartment: Apartment): void {
    const dialogRef = this.dialog.open(ApartmentDialogComponent, {
      width: '600px',
      data: { apartment, buildings: this.buildings }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadApartments(this.buildingFilter.value || undefined);
      }
    });
  }

  deleteApartment(apartment: Apartment): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer l'appartement ${apartment.apartmentNumber} ?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.apartmentService.delete(apartment.id).subscribe({
          next: () => {
            this.snackBar.open('Appartement supprimé avec succès', 'Fermer', { duration: 3000 });
            this.loadApartments(this.buildingFilter.value || undefined);
          },
          error: (error) => {
            console.error('Error deleting apartment:', error);
            const message = error.error?.message || 'Erreur lors de la suppression';
            this.snackBar.open(message, 'Fermer', { duration: 5000 });
          }
        });
      }
    });
  }

  openPaymentDialog(apartment: Apartment): void {
    const dialogRef = this.dialog.open(PaymentDialogComponent, {
      width: '700px',
      data: { apartment }
    });

    dialogRef.afterClosed().subscribe(success => {
      if (success) {
        // Rafraîchir la table pour mettre à jour les cotisations
        this.loadApartments(this.buildingFilter.value || undefined);
      }
    });
  }

  openReceiptDialog(apartment: Apartment): void {
    // Récupérer le nom de l'adhérent associé à cet appartement
    this.apartmentService.getMemberName(apartment.id).subscribe({
      next: (response: any) => {
        const dialogRef = this.dialog.open(GenerateReceiptDialogComponent, {
          width: '500px',
          data: {
            apartment,
            memberName: response.memberName
          }
        });

        dialogRef.afterClosed().subscribe(success => {
          if (success) {
            // Reçu généré avec succès
          }
        });
      },
      error: (error) => {
        console.error('Error fetching member name:', error);
        this.snackBar.open('Erreur lors de la récupération des informations', 'Fermer', { duration: 3000 });
      }
    });
  }

  viewMemberDetails(apartment: Apartment): void {
    if (!apartment.memberId) {
      return;
    }
    
    // Charger les détails du membre
    this.memberService.getById(apartment.memberId).subscribe({
      next: (member) => {
        // Ouvrir le dialog en mode lecture seule
        const dialogRef = this.dialog.open(AddEditMemberDialogComponent, {
          width: '600px',
          data: { ...member, readOnly: true },
          disableClose: false
        });

        dialogRef.afterClosed().subscribe(result => {
          // Le dialog est en lecture seule, pas besoin de rafraîchir
        });
      },
      error: (error) => {
        console.error('Error loading member details:', error);
        this.snackBar.open('Erreur lors du chargement des détails de l\'adhérent', 'Fermer', { duration: 3000 });
      }
    });
  }
}
