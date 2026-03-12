import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { NotesService } from '../../../core/services/notes.service';
import { BuildingService } from '../../../core/services/building.service';
import { ApartmentService } from '../../../core/services/apartment.service';
import { Building, Apartment } from '../../../core/models/settings.models';
import { MemberLookupForNotes, CreateMemberNoteDto } from '../../../core/models/notes.models';

@Component({
  selector: 'app-add-note-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './add-note-dialog.component.html',
  styleUrls: ['./add-note-dialog.component.scss']
})
export class AddNoteDialogComponent implements OnInit {
  noteForm: FormGroup;
  buildings: Building[] = [];
  apartments: Apartment[] = [];
  members: MemberLookupForNotes[] = [];
  loading = false;
  loadingApartments = false;
  loadingMembers = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddNoteDialogComponent>,
    private notesService: NotesService,
    private buildingService: BuildingService,
    private apartmentService: ApartmentService,
    private snackBar: MatSnackBar
  ) {
    this.noteForm = this.fb.group({
      buildingId: [null, Validators.required],
      apartmentId: [null],
      memberId: [null, Validators.required],
      noteText: ['', [Validators.required, Validators.maxLength(4000)]]
    });
  }

  ngOnInit(): void {
    this.loadBuildings();
    this.setupFormListeners();
  }

  setupFormListeners(): void {
    // When building changes, load apartments and reset apartment/member
    this.noteForm.get('buildingId')?.valueChanges.subscribe((buildingId) => {
      this.noteForm.patchValue({ apartmentId: null, memberId: null });
      this.apartments = [];
      this.members = [];
      
      if (buildingId) {
        this.loadApartments(buildingId);
        this.loadMembers(buildingId);
      }
    });

    // When apartment changes, filter members
    this.noteForm.get('apartmentId')?.valueChanges.subscribe((apartmentId) => {
      this.noteForm.patchValue({ memberId: null });
      
      const buildingId = this.noteForm.get('buildingId')?.value;
      if (buildingId) {
        this.loadMembers(buildingId, apartmentId);
      }
    });
  }

  loadBuildings(): void {
    this.buildingService.getAll().subscribe({
      next: (buildings) => {
        this.buildings = buildings;
      },
      error: (error) => {
        console.error('Error loading buildings:', error);
        this.snackBar.open('Erreur lors du chargement des immeubles', 'Fermer', { duration: 3000 });
      }
    });
  }

  loadApartments(buildingId: number): void {
    this.loadingApartments = true;
    this.apartmentService.getByBuilding(buildingId).subscribe({
      next: (apartments) => {
        this.apartments = apartments;
        this.loadingApartments = false;
      },
      error: (error) => {
        console.error('Error loading apartments:', error);
        this.snackBar.open('Erreur lors du chargement des appartements', 'Fermer', { duration: 3000 });
        this.loadingApartments = false;
      }
    });
  }

  loadMembers(buildingId: number, apartmentId?: number): void {
    this.loadingMembers = true;
    this.notesService.getMembersLookup(buildingId, apartmentId).subscribe({
      next: (members) => {
        this.members = members;
        this.loadingMembers = false;
      },
      error: (error) => {
        console.error('Error loading members:', error);
        this.snackBar.open('Erreur lors du chargement des adhérents', 'Fermer', { duration: 3000 });
        this.loadingMembers = false;
      }
    });
  }

  isMemberSelectDisabled(): boolean {
    return !this.noteForm.get('buildingId')?.value;
  }

  onSubmit(): void {
    if (this.noteForm.valid) {
      this.loading = true;
      const dto: CreateMemberNoteDto = {
        memberId: this.noteForm.value.memberId,
        noteText: this.noteForm.value.noteText
      };

      this.notesService.create(dto).subscribe({
        next: () => {
          this.snackBar.open('Note ajoutée avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('Error creating note:', error);
          let errorMessage = 'Erreur lors de l\'ajout de la note';
          if (error.error?.message) {
            errorMessage = error.error.message;
          }
          this.snackBar.open(errorMessage, 'Fermer', { duration: 5000 });
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
