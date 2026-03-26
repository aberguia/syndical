import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
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
import { CarService } from '../../../core/services/car.service';
import { BuildingService } from '../../../core/services/building.service';
import { MemberService } from '../../../core/services/member.service';
import { Car } from '../../../core/models/parking.models';
import { Building } from '../../../core/models/settings.models';
import { AddEditCarDialogComponent } from './add-edit-car-dialog.component';
import { AddEditMemberDialogComponent } from '../../settings/members/add-edit-member-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-parking-cars',
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
    TranslateModule
  ],
  templateUrl: './parking-cars.component.html',
  styleUrls: ['./parking-cars.component.scss']
})
export class ParkingCarsComponent implements OnInit {
  displayedColumns: string[] = ['brand', 'plate', 'carType', 'member', 'building', 'apartment', 'actions'];
  dataSource: MatTableDataSource<Car>;
  buildings: Building[] = [];
  searchControl = new FormControl('');
  buildingFilter = new FormControl(0);
  loading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private carService: CarService,
    private buildingService: BuildingService,
    private memberService: MemberService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.dataSource = new MatTableDataSource<Car>([]);
  }

  ngOnInit(): void {
    this.loadBuildings();
    this.loadCars();
    
    // Recherche avec debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.loadCars();
    });

    this.buildingFilter.valueChanges.subscribe(() => {
      this.loadCars();
    });
  }

  loadBuildings(): void {
    this.buildingService.getAll().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
      }
    });
  }

  loadCars(): void {
    this.loading = true;
    const search = this.searchControl.value || undefined;
    const buildingId = this.buildingFilter.value || undefined;
    
    this.carService.getAll(search, buildingId).subscribe({
      next: (cars) => {
        this.dataSource.data = cars;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading cars:', error);
        this.snackBar.open('Erreur lors du chargement des voitures', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(AddEditCarDialogComponent, {
      width: '600px',
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCars();
      }
    });
  }

  openEditDialog(car: Car): void {
    const dialogRef = this.dialog.open(AddEditCarDialogComponent, {
      width: '600px',
      data: car
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadCars();
      }
    });
  }

  deleteCar(car: Car): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer la voiture ${car.plateFormatted} ?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.carService.delete(car.id).subscribe({
          next: () => {
            this.snackBar.open('Voiture supprimée avec succès', 'Fermer', { duration: 3000 });
            this.loadCars();
          },
          error: (error) => {
            console.error('Error deleting car:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }

  getCarTypeClass(carType: number): string {
    switch (carType) {
      case 0: return 'type-primary';
      case 1: return 'type-tenant';
      case 2: return 'type-visitor';
      default: return '';
    }
  }

  viewMemberDetails(car: Car): void {
    if (!car.memberId) {
      return;
    }
    
    // Charger les détails du membre
    this.memberService.getById(car.memberId).subscribe({
      next: (member) => {
        // Ouvrir le dialog en mode lecture seule
        const dialogRef = this.dialog.open(AddEditMemberDialogComponent, {
          width: '600px',
          data: { ...member, readOnly: true },
          disableClose: false
        });
      },
      error: (error) => {
        console.error('Error loading member details:', error);
        this.snackBar.open('Erreur lors du chargement des détails de l\'adhérent', 'Fermer', { duration: 3000 });
      }
    });
  }
}
