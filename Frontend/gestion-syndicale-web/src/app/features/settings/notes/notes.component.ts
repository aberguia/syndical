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
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { NotesService } from '../../../core/services/notes.service';
import { BuildingService } from '../../../core/services/building.service';
import { ApartmentService } from '../../../core/services/apartment.service';
import { AuthService } from '../../../core/services/auth.service';
import { MemberNote } from '../../../core/models/notes.models';
import { Building, Apartment } from '../../../core/models/settings.models';
import { AddNoteDialogComponent } from './add-note-dialog.component';
import { EditNoteDialogComponent } from './edit-note-dialog.component';
import { ViewNoteDialogComponent } from './view-note-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-notes',
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
    MatTooltipModule,
    MatCardModule
  ],
  templateUrl: './notes.component.html',
  styleUrls: ['./notes.component.scss']
})
export class NotesComponent implements OnInit {
  displayedColumns: string[] = ['memberFullName', 'buildingCodeOrName', 'apartmentNumber', 'noteText', 'createdAt', 'actions'];
  dataSource: MatTableDataSource<MemberNote>;
  buildings: Building[] = [];
  apartments: Apartment[] = [];
  searchControl = new FormControl('');
  buildingFilter = new FormControl(0);
  apartmentFilter = new FormControl(0);
  loading = true;
  isSuperAdmin = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private notesService: NotesService,
    private buildingService: BuildingService,
    private apartmentService: ApartmentService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.dataSource = new MatTableDataSource<MemberNote>([]);
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
  }

  ngOnInit(): void {
    this.loadBuildings();
    this.loadNotes();
    
    this.searchControl.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => {
      this.loadNotes();
    });

    this.buildingFilter.valueChanges.subscribe((buildingId) => {
      if (buildingId && buildingId !== 0) {
        this.loadApartments(buildingId);
      } else {
        this.apartments = [];
        this.apartmentFilter.setValue(0);
      }
      this.loadNotes();
    });

    this.apartmentFilter.valueChanges.subscribe(() => {
      this.loadNotes();
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

  loadApartments(buildingId: number): void {
    this.apartmentService.getByBuilding(buildingId).subscribe({
      next: (apartments) => {
        this.apartments = apartments;
      },
      error: (error) => {
        console.error('Error loading apartments:', error);
      }
    });
  }

  loadNotes(): void {
    this.loading = true;
    const search = this.searchControl.value || undefined;
    const buildingId = this.buildingFilter.value || undefined;
    const apartmentId = this.apartmentFilter.value || undefined;
    
    this.notesService.getAll(search, buildingId, apartmentId).subscribe({
      next: (notes) => {
        this.dataSource.data = notes;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading notes:', error);
        this.snackBar.open('Erreur lors du chargement des notes', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(AddNoteDialogComponent, {
      width: '600px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadNotes();
      }
    });
  }

  openViewDialog(note: MemberNote): void {
    this.dialog.open(ViewNoteDialogComponent, {
      width: '600px',
      data: note
    });
  }

  openEditDialog(note: MemberNote): void {
    const dialogRef = this.dialog.open(EditNoteDialogComponent, {
      width: '600px',
      data: note
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadNotes();
      }
    });
  }

  deleteNote(note: MemberNote): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer cette note ?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.notesService.delete(note.id).subscribe({
          next: () => {
            this.snackBar.open('Note supprimée avec succès', 'Fermer', { duration: 3000 });
            this.loadNotes();
          },
          error: (error) => {
            console.error('Error deleting note:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }

  isNoteLong(noteText: string): boolean {
    return noteText.length > 100;
  }

  getTruncatedNote(noteText: string): string {
    return noteText.length > 100 ? noteText.substring(0, 100) + '...' : noteText;
  }
}
