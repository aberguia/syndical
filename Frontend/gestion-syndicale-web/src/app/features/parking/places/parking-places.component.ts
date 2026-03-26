import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ParkingStatusService } from '../../../core/services/parking-status.service';
import { AuthService } from '../../../core/services/auth.service';
import { ParkingStatus } from '../../../core/models/parking.models';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-parking-places',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    TranslateModule
  ],
  templateUrl: './parking-places.component.html',
  styleUrls: ['./parking-places.component.scss']
})
export class ParkingPlacesComponent implements OnInit, OnDestroy {
  status: ParkingStatus | null = null;
  loading = true;
  isSuperAdmin = false;
  manualCurrentCars: number = 0;
  
  private hubConnection: signalR.HubConnection | null = null;
  private pollingInterval: any = null;
  isSignalRConnected = false;

  constructor(
    private parkingService: ParkingStatusService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
  }

  ngOnInit(): void {
    this.loadStatus();
    this.startSignalR();
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.stopSignalR();
    this.stopPolling();
  }

  loadStatus(): void {
    this.parkingService.getStatus().subscribe({
      next: (status) => {
        this.status = status;
        this.manualCurrentCars = status.currentCars;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading parking status:', error);
        this.snackBar.open('Erreur lors du chargement du statut', 'Fermer', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  increment(): void {
    this.parkingService.increment(1).subscribe({
      next: (status) => {
        this.status = status;
        this.manualCurrentCars = status.currentCars;
      },
      error: (error) => {
        console.error('Error incrementing:', error);
        this.snackBar.open('Erreur lors de l\'entrée', 'Fermer', { duration: 3000 });
      }
    });
  }

  decrement(): void {
    if (this.status && this.status.currentCars === 0) {
      this.snackBar.open('Impossible de décrémenter (déjà à 0)', 'Fermer', { duration: 3000 });
      return;
    }

    this.parkingService.decrement(1).subscribe({
      next: (status) => {
        this.status = status;
        this.manualCurrentCars = status.currentCars;
      },
      error: (error) => {
        console.error('Error decrementing:', error);
        this.snackBar.open('Erreur lors de la sortie', 'Fermer', { duration: 3000 });
      }
    });
  }

  saveManualAdjustment(): void {
    if (this.manualCurrentCars < 0) {
      this.snackBar.open('Le nombre de voitures ne peut pas être négatif', 'Fermer', { duration: 3000 });
      return;
    }

    this.parkingService.setCurrentCars(this.manualCurrentCars).subscribe({
      next: (status) => {
        this.status = status;
        this.manualCurrentCars = status.currentCars;
        this.snackBar.open('Compteur ajusté avec succès', 'Fermer', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error setting current cars:', error);
        this.snackBar.open('Erreur lors de l\'ajustement', 'Fermer', { duration: 3000 });
      }
    });
  }

  getStatusClass(): string {
    if (!this.status) return '';
    if (this.status.status === 'Dépassé') return 'status-exceeded';
    if (this.status.status === 'Plein') return 'status-full';
    return 'status-ok';
  }

  getStatusIcon(): string {
    if (!this.status) return 'local_parking';
    if (this.status.status === 'Dépassé') return 'warning';
    if (this.status.status === 'Plein') return 'block';
    return 'check_circle';
  }

  private startSignalR(): void {
    const token = this.authService.getToken();
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api', '')}/hubs/parking`, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ParkingStatusUpdated', (status: ParkingStatus) => {
      this.status = status;
      this.manualCurrentCars = status.currentCars;
    });

    this.hubConnection.onreconnecting(() => {
      this.isSignalRConnected = false;
      this.snackBar.open('Reconnexion en cours...', 'Fermer', { duration: 2000 });
    });

    this.hubConnection.onreconnected(() => {
      this.isSignalRConnected = true;
      this.snackBar.open('Connexion temps réel rétablie', 'Fermer', { duration: 2000 });
    });

    this.hubConnection.onclose(() => {
      this.isSignalRConnected = false;
      this.snackBar.open('Connexion temps réel perdue, mode polling activé', 'Fermer', { duration: 3000 });
    });

    this.hubConnection.start()
      .then(() => {
        this.isSignalRConnected = true;
        console.log('SignalR Connected');
      })
      .catch(err => {
        console.error('SignalR Connection Error:', err);
        this.snackBar.open('Connexion temps réel indisponible, mode polling activé', 'Fermer', { duration: 3000 });
      });
  }

  private stopSignalR(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  private startPolling(): void {
    // Polling toutes les 10 secondes comme fallback
    this.pollingInterval = setInterval(() => {
      if (!this.isSignalRConnected) {
        this.parkingService.getStatus().subscribe({
          next: (status) => {
            this.status = status;
            this.manualCurrentCars = status.currentCars;
          },
          error: (error) => {
            console.error('Polling error:', error);
          }
        });
      }
    }, 10000);
  }

  private stopPolling(): void {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }
}
