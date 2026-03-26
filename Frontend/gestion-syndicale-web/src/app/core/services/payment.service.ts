import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApartmentPaidMonths, ApartmentPaymentStatus, CancelMonthlyPaymentDto, CreatePaymentDto } from '../models/payment.models';

export interface ApartmentBalance {
  apartmentId: number;
  buildingNumber: string;
  apartmentNumber: string;
  totalDue: number;
  totalPaid: number;
  balance: number;
  pendingCalls: CallForFunds[];
  recentPayments: PaymentDetail[];
}

export interface CallForFunds {
  id: number;
  chargeName: string;
  amountDue: number;
  amountPaid: number;
  amountRemaining: number;
  dueDate: Date;
  status: string;
}

export interface PaymentDetail {
  id: number;
  apartmentId: number;
  buildingNumber: string;
  apartmentNumber: string;
  amount: number;
  paymentMethod: string;
  referenceNumber?: string;
  paymentDate: Date;
  recordedAt: Date;
  recordedByName: string;
  notes?: string;
  receiptFilePath?: string;
  allocations: PaymentAllocation[];
}

export interface PaymentAllocation {
  chargeName: string;
  allocatedAmount: number;
  allocatedAt: Date;
}

export interface CreatePayment {
  apartmentId: number;
  amount: number;
  paymentMethod: string;
  referenceNumber?: string;
  paymentDate: Date;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  recordPayment(payment: CreatePayment): Observable<{ message: string; paymentId: number }> {
    return this.http.post<{ message: string; paymentId: number }>(this.apiUrl, payment);
  }

  getPaymentById(id: number): Observable<PaymentDetail> {
    return this.http.get<PaymentDetail>(`${this.apiUrl}/${id}`);
  }

  getPaymentsByApartment(apartmentId: number, page: number = 1, pageSize: number = 10): Observable<PaymentDetail[]> {
    return this.http.get<PaymentDetail[]>(`${this.apiUrl}/apartment/${apartmentId}?page=${page}&pageSize=${pageSize}`);
  }

  getApartmentBalance(apartmentId: number): Observable<ApartmentBalance> {
    return this.http.get<ApartmentBalance>(`${this.apiUrl}/apartment/${apartmentId}/balance`);
  }

  downloadReceipt(paymentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${paymentId}/receipt`, { responseType: 'blob' });
  }

  /**
   * Récupère l'état global des paiements pour un appartement
   */
  getPaymentStatus(apartmentId: number): Observable<ApartmentPaymentStatus> {
    return this.http.get<ApartmentPaymentStatus>(`${this.apiUrl}/apartment/${apartmentId}/status`);
  }

  /**
   * Récupère les mois payés pour un appartement et une année donnée
   */
  getPaidMonthsByApartmentAndYear(apartmentId: number, year: number): Observable<ApartmentPaidMonths> {
    return this.http.get<ApartmentPaidMonths>(`${this.apiUrl}/apartment/${apartmentId}/year/${year}`);
  }

  /**
   * Enregistre un paiement mensuel pour plusieurs mois
   */
  createMonthlyPayment(dto: CreatePaymentDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/apartment`, dto);
  }

  cancelMonthlyPayments(dto: CancelMonthlyPaymentDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/monthly/cancel`, dto);
  }

  /**
   * Récupère le résumé des paiements pour tous les appartements (1 seule requête optimisée)
   */
  getPaymentsSummaryByYear(year: number): Observable<PaymentsSummary> {
    return this.http.get<PaymentsSummary>(`${this.apiUrl}/summary/year/${year}`);
  }

  /**
   * Récupère le résumé des paiements pour tous les immeubles (1 seule requête optimisée)
   */
  getBuildingsPaymentsSummaryByYear(year: number): Observable<BuildingsPaymentsSummary> {
    return this.http.get<BuildingsPaymentsSummary>(`${this.apiUrl}/summary/buildings/year/${year}`);
  }

  /**
   * Récupère la liste de tous les immeubles
   */
  getBuildings(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/buildings`);
  }

  /**
   * Récupère la liste de tous les appartements
   */
  getApartments(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/apartments`);
  }
}

export interface PaymentsSummary {
  year: number;
  apartments: ApartmentPaymentSummary[];
}

export interface ApartmentPaymentSummary {
  apartmentId: number;
  paidMonthsCount: number;
  previousYearsUnpaidCount: number;
}

export interface BuildingsPaymentsSummary {
  year: number;
  buildings: BuildingPaymentSummary[];
}

export interface BuildingPaymentSummary {
  buildingId: number;
  apartmentsCount: number;
  totalPaidMonths: number;
}
