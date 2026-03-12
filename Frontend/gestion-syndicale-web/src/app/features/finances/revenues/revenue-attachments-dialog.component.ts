import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { RevenuesService } from '../../../core/services/revenues.service';
import { OtherRevenueDetail, RevenueDocument } from '../../../core/models/revenues.models';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';

@Component({
  selector: 'app-revenue-attachments-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  template: `
    <h2 mat-dialog-title>Pièces jointes - {{ revenueTitle }}</h2>

    <mat-dialog-content>
      <div class="upload-section">
        <input type="file" #fileInput hidden multiple accept=".jpg,.jpeg,.png,.pdf"
               (change)="onFileSelected($event)">
        <button mat-raised-button color="primary" (click)="fileInput.click()">
          <mat-icon>upload</mat-icon>
          Ajouter des fichiers
        </button>
        <p class="upload-hint">Formats acceptés: JPG, PNG, PDF (max 10 MB par fichier)</p>
      </div>

      <div class="attachments-list" *ngIf="attachments.length > 0 && !loading">
        <mat-list>
          <mat-list-item *ngFor="let doc of attachments">
            <mat-icon matListItemIcon>{{ getFileIcon(doc.fileType) }}</mat-icon>
            <div matListItemTitle>{{ doc.fileName }}</div>
            <div matListItemLine>{{ formatFileSize(doc.fileSize) }} - {{ doc.uploadedAt | date:'dd/MM/yyyy HH:mm' }}</div>
            <div matListItemMeta class="actions">
              <button mat-icon-button (click)="downloadFile(doc)" matTooltip="Télécharger">
                <mat-icon>download</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteFile(doc)" matTooltip="Supprimer">
                <mat-icon>delete</mat-icon>
              </button>
            </div>
          </mat-list-item>
        </mat-list>
      </div>

      <div class="no-attachments" *ngIf="attachments.length === 0 && !loading">
        <mat-icon>attach_file</mat-icon>
        <p>Aucune pièce jointe</p>
      </div>

      <div class="loading-container" *ngIf="loading">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="onClose()">Fermer</button>
    </mat-dialog-actions>
  `,
  styles: [`
    mat-dialog-content {
      min-width: 700px;
      min-height: 300px;
    }

    .upload-section {
      padding: 16px;
      background-color: #f5f5f5;
      border-radius: 4px;
      margin-bottom: 16px;
      text-align: center;

      button {
        mat-icon {
          margin-right: 8px;
        }
      }

      .upload-hint {
        margin: 8px 0 0 0;
        font-size: 12px;
        color: #666;
      }
    }

    .attachments-list {
      mat-list-item {
        border-bottom: 1px solid #eee;

        &:last-child {
          border-bottom: none;
        }

        .actions {
          display: flex;
          gap: 4px;
        }
      }
    }

    .no-attachments {
      text-align: center;
      padding: 40px;
      color: #999;

      mat-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        opacity: 0.3;
      }

      p {
        margin-top: 16px;
        font-style: italic;
      }
    }

    .loading-container {
      display: flex;
      justify-content: center;
      padding: 40px;
    }
  `]
})
export class RevenueAttachmentsDialogComponent implements OnInit {
  revenueId: number;
  revenueTitle: string;
  attachments: RevenueDocument[] = [];
  loading = false;

  constructor(
    private revenuesService: RevenuesService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<RevenueAttachmentsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.revenueId = data.revenueId;
    this.revenueTitle = data.revenueTitle;
  }

  ngOnInit(): void {
    this.loadAttachments();
  }

  loadAttachments(): void {
    this.loading = true;
    this.revenuesService.getOtherRevenue(this.revenueId).subscribe({
      next: (revenue) => {
        this.attachments = revenue.attachments;
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
    const files: File[] = Array.from(event.target.files);
    if (files.length === 0) return;

    const maxSize = 10 * 1024 * 1024; // 10 MB
    const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];

    for (const file of files) {
      if (file.size > maxSize) {
        this.snackBar.open(`Le fichier ${file.name} dépasse la taille maximale (10 MB)`, 'Fermer', { duration: 3000 });
        return;
      }
      if (!allowedTypes.includes(file.type)) {
        this.snackBar.open(`Le type du fichier ${file.name} n'est pas autorisé`, 'Fermer', { duration: 3000 });
        return;
      }
    }

    this.loading = true;
    this.revenuesService.uploadAttachments(this.revenueId, files).subscribe({
      next: () => {
        this.snackBar.open(`${files.length} fichier(s) téléchargé(s) avec succès`, 'Fermer', { duration: 3000 });
        this.loadAttachments();
      },
      error: (error) => {
        console.error('Error uploading files:', error);
        this.snackBar.open('Erreur lors du téléchargement', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });

    event.target.value = '';
  }

  downloadFile(doc: RevenueDocument): void {
    this.revenuesService.downloadAttachment(this.revenueId, doc.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = doc.fileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading file:', error);
        this.snackBar.open('Erreur lors du téléchargement', 'Fermer', { duration: 3000 });
      }
    });
  }

  deleteFile(doc: RevenueDocument): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer le fichier "${doc.fileName}" ?`,
        confirmText: 'Supprimer',
        cancelText: 'Annuler'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.revenuesService.deleteAttachment(this.revenueId, doc.id).subscribe({
          next: () => {
            this.snackBar.open('Fichier supprimé avec succès', 'Fermer', { duration: 3000 });
            this.loadAttachments();
          },
          error: (error) => {
            console.error('Error deleting file:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }

  getFileIcon(fileType: string): string {
    if (fileType.includes('pdf')) return 'picture_as_pdf';
    if (fileType.includes('image')) return 'image';
    return 'insert_drive_file';
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  onClose(): void {
    this.dialogRef.close(this.attachments.length > 0);
  }
}
