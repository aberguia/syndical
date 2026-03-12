export interface Car {
  id: number;
  brand: CarBrand;
  brandDisplay: string;
  platePart1: number;
  platePart2: string;
  platePart3: number;
  plateFormatted: string;
  carType: CarType;
  carTypeDisplay: string;
  memberId: number;
  memberFullName: string;
  memberPhone?: string;
  buildingId?: number;
  buildingCode?: string;
  apartmentId?: number;
  apartmentNumber?: string;
  notes?: string;
  createdAt: Date;
}

export enum CarType {
  Primary = 0,
  Tenant = 1,
  Visitor = 2
}

export enum CarBrand {
  Dacia = 0,
  Renault = 1,
  Peugeot = 2,
  Citroen = 3,
  Hyundai = 4,
  Kia = 5,
  Toyota = 6,
  Volkswagen = 7,
  Mercedes = 8,
  BMW = 9,
  Audi = 10,
  Ford = 11,
  Fiat = 12,
  Nissan = 13,
  Suzuki = 14,
  Opel = 15,
  Seat = 16,
  Skoda = 17,
  Mazda = 18,
  Mitsubishi = 19,
  Autre = 99
}

export interface CreateCarDto {
  brand: CarBrand;
  platePart1: number;
  platePart2: string;
  platePart3: number;
  carType: CarType;
  memberId: number;
  notes?: string;
}

export interface UpdateCarDto {
  brand: CarBrand;
  platePart1: number;
  platePart2: string;
  platePart3: number;
  carType: CarType;
  memberId: number;
  notes?: string;
}

export interface MemberLookup {
  id: number;
  fullName: string;
  buildingCode?: string;
  apartmentNumber?: string;
  displayText: string;
}

export interface ParkingStatus {
  totalPlaces: number;
  currentCars: number;
  availablePlaces: number;
  status: string; // "OK" | "Plein" | "Dépassé"
  updatedAt: Date;
}

export interface IncrementDecrementDto {
  count: number;
}

export interface SetCurrentCarsDto {
  currentCars: number;
}
