export interface BuildingRankingItem {
  buildingId: number;
  buildingName: string;
  paidAmount: number;
  expectedAmount: number;
  collectionRate: number;
  rank: number;
}

export interface BuildingRanking {
  myBuildingId: number;
  myBuildingRank: number;
  myBuildingRate: number;
  myPaymentStatus: 'UpToDate' | 'Late';
  items: BuildingRankingItem[];
}

// Ancien modèle pour compatibilité avec AdherentDashboard
export interface BuildingRankingOld {
  buildingId: number;
  buildingNumber: string;
  contributionPercentage: number;
  rank: number;
  isMyBuilding: boolean;
}
