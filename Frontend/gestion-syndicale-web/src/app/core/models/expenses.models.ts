// Modèles pour les dépenses

export interface Expense {
  id: number;
  expenseDate: Date;
  categoryId?: number; // Deprecated
  categoryName?: string;
  amount: number;
  description: string;
  invoiceNumber?: string;
  supplierName?: string;
  attachmentsCount: number;
  recordedAt: Date;
  recordedByName: string;
}

export interface ExpenseDetail {
  id: number;
  expenseDate: Date;
  categoryId?: number; // Deprecated
  categoryName?: string;
  supplierId?: number;
  supplierName?: string;
  amount: number;
  description: string;
  invoiceNumber?: string;
  notes?: string;
  recordedAt: Date;
  recordedByName: string;
  attachments: ExpenseAttachment[];
}

export interface CreateExpenseDto {
  expenseDate: Date;
  categoryId?: number; // Deprecated - now using supplierId
  supplierId?: number;
  amount: number;
  description: string;
  invoiceNumber?: string;
  notes?: string;
}

export interface UpdateExpenseDto {
  expenseDate: Date;
  categoryId?: number; // Deprecated - now using supplierId
  supplierId?: number;
  amount: number;
  description: string;
  invoiceNumber?: string;
  notes?: string;
}

export interface ExpensePagedResult {
  items: Expense[];
  totalCount: number;
  totalAmount: number;
  page: number;
  pageSize: number;
}

export interface ExpenseAttachment {
  id: number;
  fileName: string;
  fileType: string;
  fileSize: number;
  uploadedAt: Date;
  uploadedByName: string;
}

export interface ExpenseSummary {
  year: number;
  totalYearAmount: number;
  totalsByMonth: MonthlyExpense[];
  totalsByCategory: CategoryExpense[];
}

export interface MonthlyExpense {
  month: number;
  monthName: string;
  amount: number;
}

export interface CategoryExpense {
  categoryId: number;
  categoryName: string;
  amount: number;
}

// Catégories de dépenses

export interface ExpenseCategory {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: Date;
  expensesCount: number;
}

export interface CreateExpenseCategoryDto {
  name: string;
  description?: string;
  isActive: boolean;
}

export interface UpdateExpenseCategoryDto {
  name: string;
  description?: string;
  isActive: boolean;
}
