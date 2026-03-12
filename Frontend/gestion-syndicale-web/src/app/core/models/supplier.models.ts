export interface SupplierListDto {
  id: number;
  name: string;
  serviceCategory: string;
  description?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  createdOn: Date;
  expenseCount: number;
}

export interface SupplierDetailDto {
  id: number;
  name: string;
  serviceCategory: string;
  description?: string;
  phone?: string;
  email?: string;
  address?: string;
  isActive: boolean;
  createdOn: Date;
  createdByName?: string;
  updatedOn?: Date;
  updatedByName?: string;
}

export interface CreateSupplierDto {
  name: string;
  serviceCategory: string;
  description?: string;
  phone?: string;
  email?: string;
  address?: string;
}

export interface UpdateSupplierDto {
  name: string;
  serviceCategory: string;
  description?: string;
  phone?: string;
  email?: string;
  address?: string;
  isActive: boolean;
}

export interface SupplierLookupDto {
  id: number;
  name: string;
  serviceCategory: string;
}

export const SERVICE_CATEGORIES = [
  { value: 'Plomberie', label: 'Plomberie' },
  { value: 'Électricité', label: 'Électricité' },
  { value: 'Peinture', label: 'Peinture' },
  { value: 'Menuiserie', label: 'Menuiserie' },
  { value: 'Nettoyage', label: 'Nettoyage' },
  { value: 'Jardinage', label: 'Jardinage' },
  { value: 'Sécurité', label: 'Sécurité' },
  { value: 'Ascenseur', label: 'Ascenseur' },
  { value: 'Climatisation', label: 'Climatisation' },
  { value: 'Assurance', label: 'Assurance' },
  { value: 'Juridique', label: 'Juridique' },
  { value: 'Autre', label: 'Autre' }
];
