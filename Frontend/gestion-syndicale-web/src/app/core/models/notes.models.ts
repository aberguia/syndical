export interface MemberNote {
  id: number;
  memberId: number;
  memberFullName: string;
  buildingId?: number;
  buildingCodeOrName?: string;
  apartmentId?: number;
  apartmentNumber?: string;
  noteText: string;
  createdAt: Date;
  createdByName?: string;
}

export interface CreateMemberNoteDto {
  memberId: number;
  noteText: string;
}

export interface UpdateMemberNoteDto {
  noteText: string;
}

export interface MemberLookupForNotes {
  memberId: number;
  fullName: string;
  apartmentNumber?: string;
  buildingCodeOrName?: string;
  displayText: string;
}
