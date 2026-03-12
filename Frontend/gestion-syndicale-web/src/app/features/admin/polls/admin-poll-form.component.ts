import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PollsService } from '../../../core/services/polls.service';
import { Poll, CreatePollDto, UpdatePollDto } from '../../../core/models/community.models';

@Component({
  selector: 'app-admin-poll-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, 
            MatInputModule, MatButtonModule, MatIconModule, MatSnackBarModule],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Modifier le sondage' : 'Nouveau sondage' }}</h2>
    
    <mat-dialog-content>
      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Question</mat-label>
          <input matInput formControlName="question" placeholder="Quelle est votre question ?" maxlength="300">
          <mat-error *ngIf="form.get('question')?.hasError('required')">La question est requise</mat-error>
          <mat-error *ngIf="form.get('question')?.hasError('maxlength')">Maximum 300 caractères</mat-error>
        </mat-form-field>

        <div class="options-section">
          <h3>Options de réponse</h3>
          <div formArrayName="options">
            <div *ngFor="let option of options.controls; let i = index" class="option-row">
              <mat-form-field appearance="outline" class="option-field">
                <mat-label>Option {{ i + 1 }}</mat-label>
                <input matInput [formControlName]="i" placeholder="Libellé de l'option" maxlength="200">
                <mat-error>Cette option est requise</mat-error>
              </mat-form-field>
              <button mat-icon-button color="warn" (click)="removeOption(i)" 
                      [disabled]="options.length <= 2" type="button">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </div>
          
          <button mat-stroked-button (click)="addOption()" type="button" class="add-option-btn">
            <mat-icon>add</mat-icon>
            Ajouter une option
          </button>
          
          <p class="hint">Minimum 2 options requises</p>
        </div>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="cancel()">Annuler</button>
      <button mat-raised-button color="primary" (click)="save()" 
              [disabled]="form.invalid || options.length < 2 || saving">
        {{ saving ? 'Enregistrement...' : 'Enregistrer' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content { min-height: 400px; padding-top: 20px; }
    .full-width { width: 100%; margin-bottom: 24px; }
    .options-section { margin-top: 24px; }
    .options-section h3 { margin-bottom: 16px; color: #333; }
    .option-row { display: flex; gap: 8px; align-items: flex-start; margin-bottom: 12px; }
    .option-field { flex: 1; }
    .add-option-btn { margin-top: 8px; }
    .hint { font-size: 12px; color: #666; margin-top: 8px; }
  `]
})
export class AdminPollFormComponent implements OnInit {
  form: FormGroup;
  isEdit = false;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AdminPollFormComponent>,
    @Inject(MAT_DIALOG_DATA) public data: Poll | null,
    private pollsService: PollsService,
    private snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      question: ['', [Validators.required, Validators.maxLength(300)]],
      options: this.fb.array([], Validators.minLength(2))
    });
  }

  get options(): FormArray {
    return this.form.get('options') as FormArray;
  }

  ngOnInit() {
    if (this.data) {
      this.isEdit = true;
      this.form.patchValue({
        question: this.data.question
      });
      
      // Ajouter les options existantes
      this.data.options.forEach(opt => {
        this.options.push(this.fb.control(opt.label, [Validators.required, Validators.maxLength(200)]));
      });
    } else {
      // Ajouter 2 options vides par défaut
      this.addOption();
      this.addOption();
    }
  }

  addOption() {
    this.options.push(this.fb.control('', [Validators.required, Validators.maxLength(200)]));
  }

  removeOption(index: number) {
    if (this.options.length > 2) {
      this.options.removeAt(index);
    }
  }

  save() {
    if (this.form.invalid || this.options.length < 2) return;

    this.saving = true;
    const formValue = this.form.value;
    
    const optionsDto = formValue.options.map((label: string, index: number) => ({
      label,
      sortOrder: index
    }));

    if (this.isEdit && this.data) {
      const dto: UpdatePollDto = {
        question: formValue.question,
        options: optionsDto
      };
      
      this.pollsService.update(this.data.id, dto).subscribe({
        next: () => {
          this.snackBar.open('Sondage modifié avec succès', 'Fermer', { duration: 3000 });
          this.dialogRef.close(true);
        },
        error: () => {
          this.snackBar.open('Erreur lors de la modification', 'Fermer', { duration: 3000 });
          this.saving = false;
        }
      });
    } else {
      const dto: CreatePollDto = {
        question: formValue.question,
        options: optionsDto
      };
      
      this.pollsService.create(dto).subscribe({
        next: () => {
          this.snackBar.open('Sondage créé avec succès', 'Fermer', { duration: 3000 });
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
