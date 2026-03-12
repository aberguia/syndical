export interface MemberListDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  role: string;
  isActive: boolean;
  apartmentId?: number;
  apartmentNumber?: string;
  buildingId?: number;
  buildingNumber?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateMemberDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  apartmentId?: number;
  role: string;
}

export interface UpdateMemberDto {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  apartmentId?: number;
  role: string;
  isActive: boolean;
}

export interface ContactMemberDto {
  subject: string;
  body: string;
}
