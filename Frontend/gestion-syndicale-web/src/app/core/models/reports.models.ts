export interface FinancialSummary {
  from: string;
  to: string;
  totals: FinancialTotals;
  byMonth: MonthlyFinancial[];
  expensesByCategory: ExpenseByCategory[];
  otherRevenuesByTitle: RevenueByTitle[];
  collectionRate: CollectionRate;
}

export interface FinancialTotals {
  contributions: number;
  otherRevenues: number;
  totalRevenues: number;
  expenses: number;
  netResult: number;
}

export interface MonthlyFinancial {
  month: string; // "YYYY-MM"
  contributions: number;
  otherRevenues: number;
  totalRevenues: number;
  expenses: number;
  netResult: number;
}

export interface ExpenseByCategory {
  categoryId: number;
  categoryName: string;
  amount: number;
  percent: number;
}

export interface RevenueByTitle {
  title: string;
  amount: number;
  percent: number;
}

export interface CollectionRate {
  expectedAmount: number;
  collectedAmount: number;
  rate: number; // Percentage 0-100
}

export interface GeneratePdfRequest {
  from: string;
  to: string;
  lang?: string;
}
