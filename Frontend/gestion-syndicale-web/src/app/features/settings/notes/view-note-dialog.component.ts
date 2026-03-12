import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MemberNote } from '../../../core/models/notes.models';

@Component({
  selector: 'app-view-note-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule
  ],
  templateUrl: './view-note-dialog.component.html',
  styleUrls: ['./view-note-dialog.component.scss']
})
export class ViewNoteDialogComponent {
  constructor(
    private dialogRef: MatDialogRef<ViewNoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MemberNote
  ) {}

  onClose(): void {
    this.dialogRef.close();
  }
}
