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
import { MemberService } from '../../../core/services/member.service';
import { AuthService } from '../../../core/services/auth.service';
import { MemberListDto } from '../../../core/models/member.models';
import { AddEditMemberDialogComponent } from './add-edit-member-dialog.component';
import { ContactMemberDialogComponent } from './contact-member-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-members',
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
  templateUrl: './members.component.html',
  styleUrls: ['./members.component.scss']
})
export class MembersComponent implements OnInit {
  displayedColumns: string[] = ['fullName', 'email', 'phoneNumber', 'role', 'building', 'apartment', 'isActive', 'actions'];
  dataSource: MatTableDataSource<MemberListDto>;
  searchControl = new FormControl('');
  roleFilter = new FormControl('');
  loading = true;
  isSuperAdmin = false;
  isAdmin = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private memberService: MemberService,
    private authService: AuthService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.dataSource = new MatTableDataSource<MemberListDto>([]);
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
  }

  ngOnInit(): void {
    this.loadMembers();

    // Configurer le filtre de recherche
    this.searchControl.valueChanges.subscribe(value => {
      this.applyFilter(value || '');
    });

    // Configurer le filtre par rôle
    this.roleFilter.valueChanges.subscribe(() => {
      this.applyFilters();
    });
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  loadMembers(): void {
    this.loading = true;
    this.memberService.getAll().subscribe({
      next: (members) => {
        this.dataSource.data = members;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading members:', error);
        this.snackBar.open('Erreur lors du chargement des adhérents', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  applyFilter(filterValue: string): void {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  applyFilters(): void {
    const searchValue = this.searchControl.value || '';
    const roleValue = this.roleFilter.value || '';

    this.dataSource.filterPredicate = (data: MemberListDto, filter: string) => {
      const matchesSearch = 
        data.firstName.toLowerCase().includes(searchValue.toLowerCase()) ||
        data.lastName.toLowerCase().includes(searchValue.toLowerCase()) ||
        data.email.toLowerCase().includes(searchValue.toLowerCase());

      const matchesRole = !roleValue || data.role === roleValue;

      return matchesSearch && matchesRole;
    };

    this.dataSource.filter = Math.random().toString();
  }

  openAddDialog(): void {
    const dialogRef = this.dialog.open(AddEditMemberDialogComponent, {
      width: '600px',
      data: null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadMembers();
      }
    });
  }

  openEditDialog(member: MemberListDto): void {
    const dialogRef = this.dialog.open(AddEditMemberDialogComponent, {
      width: '600px',
      data: member
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadMembers();
      }
    });
  }

  openContactDialog(member: MemberListDto): void {
    const dialogRef = this.dialog.open(ContactMemberDialogComponent, {
      width: '500px',
      data: member
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open('Email envoyé avec succès', 'Fermer', { duration: 3000 });
      }
    });
  }

  deleteMember(member: MemberListDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Confirmer la suppression',
        message: `Êtes-vous sûr de vouloir supprimer l'adhérent ${member.firstName} ${member.lastName} ?`,
        confirmText: 'Supprimer',
        cancelText: 'Annuler'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.memberService.delete(member.id).subscribe({
          next: () => {
            this.snackBar.open('Adhérent supprimé avec succès', 'Fermer', { duration: 3000 });
            this.loadMembers();
          },
          error: (error) => {
            console.error('Error deleting member:', error);
            this.snackBar.open('Erreur lors de la suppression', 'Fermer', { duration: 3000 });
          }
        });
      }
    });
  }

  getFullName(member: MemberListDto): string {
    return `${member.firstName} ${member.lastName}`;
  }

  getBuildingApartment(member: MemberListDto): string {
    if (member.buildingNumber && member.apartmentNumber) {
      return `${member.buildingNumber} - App ${member.apartmentNumber}`;
    }
    return 'Non attribué';
  }

  getRoleLabel(role: string): string {
    const roleLabels: { [key: string]: string } = {
      'SuperAdmin': 'Super Admin',
      'Admin': 'Administrateur',
      'User': 'Adhérent'
    };
    return roleLabels[role] || role;
  }

  getRoleClass(role: string): string {
    const roleClasses: { [key: string]: string } = {
      'SuperAdmin': 'role-superadmin',
      'Admin': 'role-admin',
      'User': 'role-user'
    };
    return roleClasses[role] || '';
  }
}
