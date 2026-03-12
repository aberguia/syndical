// Models pour Buildings et Apartments

export interface Building {
  id: number;
  residenceId: number;
  buildingNumber: string;
  name: string;
  floorCount: number;
  isActive: boolean;
  createdAt: Date;
  apartmentsCount?: number;
}

export interface BuildingDto {
  id?: number;
  buildingNumber: string;
  name: string;
  floorCount: number;
  isActive: boolean;
}

export interface Apartment {
  id: number;
  buildingId: number;
  buildingName?: string;
  buildingNumber?: string;
  apartmentNumber: string;
  floor: number;
  surface?: number;
  sharesCount: number;
  isActive: boolean;
  createdAt: Date;
  memberFullName?: string;
  memberId?: number;
}

export interface ApartmentDto {
  id?: number;
  buildingId: number;
  apartmentNumber: string;
  floor: number;
  surface?: number;
  sharesCount: number;
  isActive: boolean;
}
