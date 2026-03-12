import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PaymentService, ApartmentBalance } from '../../../core/services/payment.service';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';

@Component({
  selector: 'app-member-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './member-dashboard.component.html',
  styleUrls: ['./member-dashboard.component.scss']
})
export class MemberDashboardComponent implements OnInit {
  balance?: ApartmentBalance;
  notifications: Notification[] = [];
  loading = true;
  user = this.authService.getCurrentUser();

  constructor(
    private paymentService: PaymentService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    console.log('Dashboard loading, user:', this.user);
    this.loadDashboard();
  }

  loadDashboard(): void {
    const apartmentId = this.user?.apartmentId;
    
    // Si SuperAdmin sans appartement, afficher un message
    if (!apartmentId) {
      this.loading = false;
      console.log('SuperAdmin sans appartement');
      return;
    }

    this.loading = true;

    // Charger la situation financière
    this.paymentService.getApartmentBalance(apartmentId).subscribe({
      next: (balance) => {
        this.balance = balance;
      },
      error: (error) => {
        console.error('Erreur chargement balance:', error);
      }
    });

    // Charger les notifications
    this.notificationService.getNotifications(false, 1, 5).subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.loading = false;
      },
      error: (error) => {
        console.error('Erreur chargement notifications:', error);
        this.loading = false;
      }
    });
  }

  downloadReceipt(paymentId: number): void {
    this.paymentService.downloadReceipt(paymentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Recu_${paymentId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Erreur téléchargement reçu:', error);
      }
    });
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Paid': return 'green';
      case 'PartiallyPaid': return 'orange';
      case 'Overdue': return 'red';
      default: return 'gray';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Paid': return 'Payé';
      case 'PartiallyPaid': return 'Partiellement payé';
      case 'Overdue': return 'En retard';
      case 'Pending': return 'En attente';
      default: return status;
    }
  }
}
