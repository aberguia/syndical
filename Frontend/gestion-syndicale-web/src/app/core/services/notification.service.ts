import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';

export interface Notification {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: Date;
  relatedEntityType?: string;
  relatedEntityId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notifications`;
  private hubConnection?: signalR.HubConnection;

  constructor(private http: HttpClient) {}

  // SignalR pour notifications en temps réel
  startConnection(userId: number): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.signalRUrl}/notifications`, {
        accessTokenFactory: () => localStorage.getItem('auth_token') || ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('SignalR Connection Error: ', err));

    this.hubConnection.on('ReceiveNotification', (notification: Notification) => {
      // Traiter la nouvelle notification
      console.log('New notification:', notification);
      // Émettre un événement ou mettre à jour un BehaviorSubject
    });
  }

  stopConnection(): void {
    this.hubConnection?.stop();
  }

  getNotifications(unreadOnly: boolean = false, page: number = 1, pageSize: number = 20): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${this.apiUrl}?unreadOnly=${unreadOnly}&page=${page}&pageSize=${pageSize}`);
  }

  markAsRead(notificationId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${notificationId}/mark-read`, {});
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`);
  }
}
