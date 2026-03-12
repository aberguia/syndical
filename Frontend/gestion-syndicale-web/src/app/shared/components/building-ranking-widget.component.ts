import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BuildingRankingService } from '../../core/services/building-ranking.service';
import { BuildingRanking, BuildingRankingItem } from '../../core/models/building-ranking.model';

@Component({
  selector: 'app-building-ranking-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './building-ranking-widget.component.html',
  styleUrls: ['./building-ranking-widget.component.scss']
})
export class BuildingRankingWidgetComponent implements OnInit, OnDestroy {
  loading = true;
  error = false;
  ranking: BuildingRanking | null = null;
  
  topThree: BuildingRankingItem[] = [];
  topTen: BuildingRankingItem[] = [];
  myBuilding: BuildingRankingItem | null = null;
  
  // For animations
  animatedRates: Map<number, number> = new Map();
  animationFrameId: number | null = null;
  
  // Gamification message
  message = '';
  messageIcon = '';

  // Mobile carousel
  currentCarouselIndex = 0;

  constructor(
    private buildingRankingService: BuildingRankingService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadRanking();
  }

  ngOnDestroy(): void {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  loadRanking(): void {
    this.loading = true;
    this.error = false;

    this.buildingRankingService.getBuildingRanking().subscribe({
      next: (data) => {
        this.ranking = data;
        this.processRankingData();
        this.generateMessage();
        this.startAnimations();
        this.loading = false;
      },
      error: (err) => {
        console.error('Erreur lors du chargement du classement:', err);
        this.error = true;
        this.loading = false;
      }
    });
  }

  private processRankingData(): void {
    if (!this.ranking) return;

    // Extract top 3
    this.topThree = this.ranking.items.slice(0, 3);
    
    // Extract top 10
    this.topTen = this.ranking.items.slice(0, 10);
    
    // Find my building
    this.myBuilding = this.ranking.items.find(
      item => item.buildingId === this.ranking!.myBuildingId
    ) || null;
  }

  private generateMessage(): void {
    if (!this.ranking) return;

    const rank = this.ranking.myBuildingRank;
    const totalBuildings = this.ranking.items.length;
    const topPercentile = Math.ceil(totalBuildings * 0.3);
    const bottomPercentile = Math.floor(totalBuildings * 0.75);

    if (rank === 1) {
      this.message = 'Votre immeuble est le premier, félicitations !';
      this.messageIcon = '🎉';
    } else if (rank <= topPercentile) {
      this.message = 'Très bien, continuez comme ça !';
      this.messageIcon = '✅';
    } else if (rank <= bottomPercentile) {
      this.message = 'Votre immeuble peut faire mieux !';
      this.messageIcon = '💪';
    } else {
      this.message = 'Votre immeuble est en bas du classement. Un effort collectif peut le faire remonter.';
      this.messageIcon = '⚠️';
    }

    // Add personal message if payment late
    if (this.ranking.myPaymentStatus === 'Late') {
      this.message += ' ℹ️ Votre cotisation n\'est pas à jour. La régulariser aidera votre immeuble à remonter au classement.';
    }
  }

  private startAnimations(): void {
    if (!this.ranking) return;

    // Initialize all rates at 0
    this.ranking.items.forEach(item => {
      this.animatedRates.set(item.buildingId, 0);
    });

    const duration = 1500; // 1.5 seconds
    const startTime = performance.now();

    const animate = (currentTime: number) => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);
      
      // Easing function (ease-out cubic)
      const easedProgress = 1 - Math.pow(1 - progress, 3);

      this.ranking!.items.forEach(item => {
        const targetRate = item.collectionRate;
        const animatedRate = targetRate * easedProgress;
        this.animatedRates.set(item.buildingId, animatedRate);
      });

      this.cdr.detectChanges();

      if (progress < 1) {
        this.animationFrameId = requestAnimationFrame(animate);
      } else {
        this.animationFrameId = null;
      }
    };

    this.animationFrameId = requestAnimationFrame(animate);
  }

  getAnimatedRate(buildingId: number): number {
    return this.animatedRates.get(buildingId) || 0;
  }

  getTubeHeight(rate: number): number {
    // Max height is 200px for 100%
    return (rate / 100) * 200;
  }

  getPodiumHeight(index: number): number {
    // Podium heights: 1st = 200px, 2nd = 160px, 3rd = 120px
    const heights = [200, 160, 120];
    return heights[index] || 100;
  }

  getPodiumOrder(index: number): number {
    // Visual order: 2nd, 1st, 3rd (center is highest)
    const orders = [2, 1, 3];
    return orders[index] || index + 1;
  }

  isMyBuilding(buildingId: number): boolean {
    return this.ranking?.myBuildingId === buildingId;
  }

  getRankBadgeColor(rank: number): string {
    if (rank === 1) return 'gold';
    if (rank === 2) return 'silver';
    if (rank === 3) return 'bronze';
    return 'default';
  }

  // Mobile carousel methods
  prevCarousel(): void {
    if (this.currentCarouselIndex > 0) {
      this.currentCarouselIndex--;
    }
  }

  nextCarousel(): void {
    if (this.currentCarouselIndex < 2) {
      this.currentCarouselIndex++;
    }
  }

  goToCarouselIndex(index: number): void {
    this.currentCarouselIndex = index;
  }

  // Format currency
  formatAmount(amount: number): string {
    return new Intl.NumberFormat('fr-MA', {
      style: 'currency',
      currency: 'MAD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }
}
