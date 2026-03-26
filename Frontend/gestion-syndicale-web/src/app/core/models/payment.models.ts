// =============================================
// Modèles pour la gestion des paiements mensuels
// =============================================

export interface ApartmentPaidMonths {
  apartmentId: number;
  year: number;
  paidMonths: number[]; // Liste des mois payés (1-12)
  firstUnpaidYear?: number | null;
  firstUnpaidMonth?: number | null;
}

export interface ApartmentPaymentStatus {
  apartmentId: number;
  firstUnpaidYear?: number | null;
  firstUnpaidMonth?: number | null;
  lastPaidYear?: number | null;
  lastPaidMonth?: number | null;
}

export interface CreatePaymentDto {
  apartmentId: number;
  year: number;
  months: number[]; // Liste des mois à payer (1-12)
}

export interface MonthCheckbox {
  monthNumber: number;
  monthName: string;
  isPaid: boolean;
  isDisabled: boolean;
  isSelected: boolean;
  markedForRemoval: boolean; // mois payé que l'user veut annuler
}

export interface CancelMonthlyPaymentDto {
  apartmentId: number;
  year: number;
  months: number[];
}

export const MONTHS_FR = [
  'Janvier',
  'Février',
  'Mars',
  'Avril',
  'Mai',
  'Juin',
  'Juillet',
  'Août',
  'Septembre',
  'Octobre',
  'Novembre',
  'Décembre'
];
