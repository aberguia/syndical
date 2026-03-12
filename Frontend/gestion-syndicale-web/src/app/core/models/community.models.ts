// Announcements Models
export interface Announcement {
  id: number;
  title: string;
  body: string;
  status: string;
  createdByName: string;
  createdOn: Date;
  updatedByName?: string;
  updatedOn?: Date;
}

export interface CreateAnnouncementDto {
  title: string;
  body: string;
}

export interface UpdateAnnouncementDto {
  title: string;
  body: string;
}

export interface AnnouncementListDto {
  totalCount: number;
  items: Announcement[];
}

// Polls Models
export interface Poll {
  id: number;
  question: string;
  status: string;
  createdByName: string;
  createdOn: Date;
  closedOn?: Date;
  options: PollOption[];
  results: PollResult[];
}

export interface PollOption {
  id: number;
  label: string;
  sortOrder: number;
}

export interface PollResult {
  optionId: number;
  label: string;
  voteCount: number;
  percentage: number;
}

export interface CreatePollDto {
  question: string;
  options: CreatePollOptionDto[];
}

export interface CreatePollOptionDto {
  label: string;
  sortOrder: number;
}

export interface UpdatePollDto {
  question: string;
  options: UpdatePollOptionDto[];
}

export interface UpdatePollOptionDto {
  id?: number;
  label: string;
  sortOrder: number;
}

export interface PollListDto {
  totalCount: number;
  items: Poll[];
}

export interface PortalPoll {
  id: number;
  question: string;
  status: string;
  createdOn: Date;
  closedOn?: Date;
  options: PollOption[];
  results: PollResult[];
  hasVoted: boolean;
  myVoteOptionId?: number;
  totalVotes: number;
  selectedOptionId?: number; // For UI binding
}

export interface PollVoteDto {
  pollId: number;
  pollOptionId: number;
}

// Dashboard Models
export interface AdminDashboard {
  kpis: Kpis;
  recentAnnouncements: RecentAnnouncement[];
  recentPolls: RecentPoll[];
  recentExpenses: RecentExpense[];
  recentRevenues: RecentRevenue[];
  parkingLive: ParkingLive;
}

export interface Kpis {
  buildingsCount: number;
  apartmentsCount: number;
  adherentsCount: number;
  totalRevenuesCurrentYear: number;
  totalExpensesCurrentYear: number;
}

export interface RecentAnnouncement {
  id: number;
  title: string;
  status: string;
  createdOn: Date;
}

export interface RecentPoll {
  id: number;
  question: string;
  status: string;
  totalVotes: number;
  createdOn: Date;
}

export interface RecentExpense {
  id: number;
  description: string;
  amount: number;
  expenseDate: Date;
}

export interface RecentRevenue {
  type: string;
  description: string;
  amount: number;
  date: Date;
}

export interface ParkingLive {
  totalPlaces: number;
  occupiedPlaces: number;
  availablePlaces: number;
}

export interface AdherentDashboard {
  contribution: ApartmentContribution;
  buildingsRanking: BuildingRanking[];
  parkingLive: ParkingLive;
  message: string;
}

export interface ApartmentContribution {
  apartmentId: number;
  apartmentNumber: string;
  buildingNumber: string;
  annualAmount: number;
  paidAmount: number;
  remainingAmount: number;
  status: string;
  isUpToDate: boolean;
}

export interface BuildingRanking {
  buildingId: number;
  buildingNumber: string;
  contributionPercentage: number;
  rank: number;
  isMyBuilding: boolean;
}
