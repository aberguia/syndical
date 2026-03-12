// Modèles pour les revenus

export interface ContributionsMatrix {
  year: number;
  buildings: BuildingMonthlyContributions[];
  monthlyTotals: number[];
  yearTotal: number;
  monthlyContributionAmount: number;
}

export interface BuildingMonthlyContributions {
  buildingId: number;
  buildingCode: string;
  monthlyAmounts: number[];
  rowTotal: number;
}

export interface OtherRevenue {
  id: number;
  revenueDate: Date;
  title: string;
  amount: number;
  description?: string;
  attachmentsCount: number;
  recordedAt: Date;
  recordedByName: string;
}

export interface OtherRevenueDetail {
  id: number;
  revenueDate: Date;
  title: string;
  amount: number;
  description?: string;
  recordedAt: Date;
  recordedByUserId: number;
  recordedByName: string;
  attachments: RevenueDocument[];
}

export interface RevenueDocument {
  id: number;
  fileName: string;
  fileType: string;
  fileSize: number;
  uploadedAt: Date;
  uploadedByName: string;
}

export interface OtherRevenuePagedResult {
  items: OtherRevenue[];
  totalCount: number;
  totalAmount: number;
}

export interface RevenuesTotal {
  year: number;
  contributionsTotal: number;
  otherRevenuesTotal: number;
  grandTotal: number;
}

export interface CreateOtherRevenueDto {
  revenueDate: Date;
  title: string;
  amount: number;
  description?: string;
}

export interface UpdateOtherRevenueDto {
  revenueDate: Date;
  title: string;
  amount: number;
  description?: string;
}
