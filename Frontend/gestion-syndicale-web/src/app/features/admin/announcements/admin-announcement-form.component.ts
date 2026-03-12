import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AnnouncementsService } from '../../../core/services/announcements.service';
import { Announcement, CreateAnnouncementDto, UpdateAnnouncementDto } from '../../../core/models/community.models';

@Component({
  selector: 'app-admin-announcement-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, 
            MatInputModule, MatButtonModule, MatSnackBarModule],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? "Modifier l'annonce" : "Nouvelle annonce" }}</h2>
    
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Titre</mat-label>
          <input matInput formControlName="title" placeholder="Titre de l'annonce" maxlength="200">
          <mat-error *ngIf="form.get('title')?.hasError('required')">Le titre est requis</mat-error>
          <mat-error *ngIf="form.get('title')?.hasError('maxlength')">Maximum 200 caractères</mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Contenu</mat-label>
          <textarea matInput formControlName="body" rows="10" placeholder="Contenu de l'annonce"></textarea>
          <mat-error *ngIf="form.get('body')?.hasError('required')">Le contenu est requis</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">Annuler</button>
      <button mat-raised-button color="primary" (click)="save()" [disabled]="form.invalid || saving">
        {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content { min-height: 300px; padding-top: 20px; }
    .full-width { width: 100%; margin-bottom: 16px; }
    textarea { min-height: 200px; }
  `]
})
export class AdminAnnouncementFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AdminAnnouncementFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Announcement | null,
    private announcementsService: AnnouncementsService,
    private snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      body: ['', Validators.required]
    });
  }

  ngOnInit() {
    if (this.data) {
      this.isEdit = true;
      this.form.patchValue({
        title: this.data.title,
        body: this.data.body
      });
    }
  }

  save() {
    if (this.form.invalid) return;

    this.saving = true;
    const formValue = this.form.value;

    if (this.isEdit && this.data) {
      const dto: UpdateAnnouncementDto = {
        title: formValue.title,
        body: formValue.body
      };
      
      this.announcementsService.update(this.data.id, dto).subscribe({
        next: () => {
          this.snackBar.open('Annonce modifiée avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: () => {
          this.snackBar.open('Erreur lors de la modification', 'Fermer', { duration: 3000 });
          this.saving = false;
        }
      });
    } else {
      const dto: CreateAnnouncementDto = {
        title: formValue.title,
        body: formValue.body
      };
      
      this.announcementsService.create(dto).subscribe({
        next: () => {
          this.snackBar.open('Annonce créée avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: () => {
          this.snackBar.open('Erreur lors de la création', 'Fermer', { duration: 3000 });
          this.saving = false;
        }
      });
    }
  }

  cancel() {
    this.dialogRef.close(false);
  }
}
