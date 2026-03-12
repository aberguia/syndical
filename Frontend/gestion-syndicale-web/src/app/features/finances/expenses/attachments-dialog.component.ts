import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ExpensesService } from '../../../core/services/expenses.service';
import { AuthService } from '../../../core/services/auth.service';
import { ExpenseAttachment } from '../../../core/models/expenses.models';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-attachments-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './attachments-dialog.component.html',
  styleUrl: './attachments-dialog.component.scss'
})
export class AttachmentsDialogComponent implements OnInit {
  attachments: ExpenseAttachment[] = [];
  loading = true;
  uploading = false;
  isSuperAdmin = false;

  constructor(
    private dialogRef: MatDialogRef<AttachmentsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { expenseId: number; expenseDescription: string },
    private expensesService: ExpensesService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
  }

  ngOnInit(): void {
    this.loadAttachments();
  }

  loadAttachments(): void {
    this.loading = true;
    this.expensesService.getAttachments(this.data.expenseId).subscribe({
      next: (attachments) => {
        this.attachments = attachments;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading attachments:', error);
        this.snackBar.open('Erreur lors du chargement des pièces jointes', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadFile(file);
    }
  }

  uploadFile(file: File): void {
    // Validation
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf'];
    if (!allowedTypes.includes(file.type)) {
      this.snackBar.open('Type de fichier non autorisé. Formats acceptés : JPG, PNG, PDF', 'Fermer', { duration: 5000 });
      return;
    }

    const maxSize = 5 * 1024 * 1024; // 5MB
    if (file.size > maxSize) {
      this.snackBar.open('Fichier trop volumineux. Taille maximale : 5MB', 'Fermer', { duration: 5000 });
      return;
    }

    this.uploading = true;
    this.expensesService.uploadAttachment(this.data.expenseId, file).subscribe({
      next: () => {
        this.snackBar.open('Fichier uploadé avec succès', 'Fermer', { duration: 3000 });
        this.loadAttachments();
        this.uploading = false;
      },
      error: (error) => {
        console.error('Error uploading file:', error);
        const message = error.error?.message || 'Erreur lors de l\'upload';
        this.snackBar.open(message, 'Fermer', { duration: 5000 });
        this.uploading = false;
      }
    });
  }

  downloadAttachment(attachment: ExpenseAttachment): void {
    this.expensesService.downloadAttachment(attachment.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = attachment.fileName;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading attachment:', error);
        this.snackBar.open('Erreur lors du téléchargement', 'Fermer', { duration: 3000 });
      }
    });
  }

  deleteAttachment(attachment: ExpenseAttachment): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer le fichier "${attachment.fileName}" ?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.expensesService.deleteAttachment(this.data.expenseId, attachment.id).subscribe({
          next: () => {
            this.snackBar.open('Pièce jointe supprimée avec succès', 'Fermer', { duration: 3000 });
            this.loadAttachments();
          },
          error: (error) => {
            console.error('Error deleting attachment:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }

  getFileIcon(fileType: string): string {
    if (fileType.includes('pdf')) {
      return 'picture_as_pdf';
    }
    return 'image';
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
