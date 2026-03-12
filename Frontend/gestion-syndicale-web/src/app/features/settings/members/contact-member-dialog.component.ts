import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MemberService } from '../../../core/services/member.service';
import { MemberListDto, ContactMemberDto } from '../../../core/models/member.models';

@Component({
  selector: 'app-contact-member-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSnackBarModule
  ],
  template: `
    <h2 mat-dialog-title>Contacter {{ data.firstName }} {{ data.lastName }}</h2>
    
    <mat-dialog-content>
      <div class="member-info">
        <p><strong>Email :</strong> {{ data.email }}</p>
        <p *ngIf="data.apartmentNumber">
          <strong>Appartement :</strong> 
          {{ data.buildingNumber }} - App {{ data.apartmentNumber }}
        </p>
      </div>

      <form [formGroup]="form">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Sujet *</mat-label>
          <input matInput formControlName="subject" placeholder="Objet du message">
          <mat-error *ngIf="form.get('subject')?.hasError('required')">
            Le sujet est requis
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Message *</mat-label>
          <textarea 
            matInput 
            formControlName="body" 
            rows="8"
            placeholder="Bonjour,&#10;&#10;Votre message ici..."></textarea>
          <mat-error *ngIf="form.get('body')?.hasError('required')">
            Le message est requis
          </mat-error>
          <mat-error *ngIf="form.get('body')?.hasError('minlength')">
            Le message doit contenir au moins 10 caractères
          </mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Annuler</button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onSend()" 
        [disabled]="form.invalid || sending">
        <span *ngIf="!sending">Envoyer</span>
        <span *ngIf="sending">Envoi en cours...</span>
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    mat-dialog-content {
      min-width: 500px;
      max-width: 600px;
      padding: 20px 24px;
    }

    .member-info {
      background-color: #f5f5f5;
      padding: 12px 16px;
      border-radius: 4px;
      margin-bottom: 20px;

      p {
        margin: 4px 0;
        font-size: 14px;
      }
    }

    textarea {
      resize: vertical;
      min-height: 120px;
    }
  `]
})
export class ContactMemberDialogComponent {
  form: FormGroup;
  sending = false;

  constructor(
    private fb: FormBuilder,
    private memberService: MemberService,
    private snackBar: MatSnackBar,
    public dialogRef: MatDialogRef<ContactMemberDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MemberListDto
  ) {
    this.form = this.fb.group({
      subject: ['', [Validators.required]],
      body: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  onSend(): void {
    if (this.form.invalid) return;

    this.sending = true;
    const contactDto: ContactMemberDto = {
      subject: this.form.value.subject,
      body: this.form.value.body
    };

    this.memberService.contact(this.data.id, contactDto).subscribe({
      next: () => {
        this.snackBar.open('Email envoyé avec succès', 'Fermer', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error sending email:', error);
        this.snackBar.open(
          error.error?.message || 'Erreur lors de l\'envoi de l\'email',
          'Fermer',
          { duration: 5000 }
        );
        this.sending = false;
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
