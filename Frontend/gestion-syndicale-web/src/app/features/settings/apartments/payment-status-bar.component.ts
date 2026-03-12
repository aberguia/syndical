import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface PaymentStatusData {
  greenMonths: number;  // Mois payés
  redMonths: number;    // Mois échus non payés
  blueMonths: number;   // Mois non échus (restants)
}

@Component({
  selector: 'app-payment-status-bar',
  standalone: true,
  imports: [CommonModule, MatTooltipModule],
  template: `
    <div class="payment-status-container" [matTooltip]="tooltipText">
      <div class="payment-bar">
        <div 
          class="segment green" 
          [style.width.%]="greenPercent"
          *ngIf="greenPercent > 0">
          <span class="segment-label" *ngIf="greenPercent > 15">
            {{ displayMode === 'percentage' ? (greenPercent | number:'1.1-1') + '%' : data.greenMonths }}
          </span>
        </div>
        <div 
          class="segment red" 
          [style.width.%]="redPercent"
          *ngIf="redPercent > 0">
          <span class="segment-label" *ngIf="redPercent > 15">
            {{ displayMode === 'percentage' ? (redPercent | number:'1.1-1') + '%' : data.redMonths }}
          </span>
        </div>
        <div 
          class="segment blue" 
          [style.width.%]="bluePercent"
          *ngIf="bluePercent > 0">
          <span class="segment-label" *ngIf="bluePercent > 15">
            {{ displayMode === 'percentage' ? (bluePercent | number:'1.1-1') + '%' : data.blueMonths }}
          </span>
        </div>
      </div>
      <div class="payment-legend">
        <span class="legend-item green-text">
          {{ displayMode === 'percentage' ? (greenPercent | number:'1.1-1') + '% payé' : data.greenMonths + ' payés' }}
        </span>
        <span class="legend-item red-text" *ngIf="(displayMode === 'months' && data.redMonths > 0) || (displayMode === 'percentage' && redPercent > 0)">
          {{ displayMode === 'percentage' ? (redPercent | number:'1.1-1') + '% en retard' : data.redMonths + ' en retard' }}
        </span>
        <span class="legend-item blue-text" *ngIf="(displayMode === 'months' && data.blueMonths > 0) || (displayMode === 'percentage' && bluePercent > 0)">
          {{ displayMode === 'percentage' ? (bluePercent | number:'1.1-1') + '% restants' : data.blueMonths + ' restants' }}
        </span>
      </div>
    </div>
  `,
  styles: [`
    .payment-status-container {
      width: 100%;
      padding: 4px 0;
    }

    .payment-bar {
      display: flex;
      width: 100%;
      height: 32px;
      border-radius: 4px;
      overflow: hidden;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
      margin-bottom: 6px;
    }

    .segment {
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.3s ease;
      position: relative;
      
      &:hover {
        opacity: 0.9;
      }
    }

    .segment.green {
      background: linear-gradient(135deg, #4caf50 0%, #66bb6a 100%);
    }

    .segment.red {
      background: linear-gradient(135deg, #f44336 0%, #ef5350 100%);
    }

    .segment.blue {
      background: linear-gradient(135deg, #2196f3 0%, #42a5f5 100%);
    }

    .segment-label {
      color: white;
      font-weight: 600;
      font-size: 12px;
      text-shadow: 0 1px 2px rgba(0,0,0,0.3);
    }

    .payment-legend {
      display: flex;
      gap: 12px;
      font-size: 11px;
      justify-content: center;
      flex-wrap: wrap;
    }

    .legend-item {
      font-weight: 500;
      display: flex;
      align-items: center;
      
      &::before {
        content: '';
        display: inline-block;
        width: 8px;
        height: 8px;
        border-radius: 50%;
        margin-right: 4px;
      }
    }

    .green-text {
      color: #4caf50;
      
      &::before {
        background: #4caf50;
      }
    }

    .red-text {
      color: #f44336;
      
      &::before {
        background: #f44336;
      }
    }

    .blue-text {
      color: #2196f3;
      
      &::before {
        background: #2196f3;
      }
    }
  `]
})
export class PaymentStatusBarComponent {
  @Input() data!: PaymentStatusData;
  @Input() displayMode: 'months' | 'percentage' = 'months'; // Par défaut: afficher en mois

  get greenPercent(): number {
    return (this.data.greenMonths / 12) * 100;
  }

  get redPercent(): number {
    return (this.data.redMonths / 12) * 100;
  }

  get bluePercent(): number {
    return (this.data.blueMonths / 12) * 100;
  }

  get tooltipText(): string {
    if (this.displayMode === 'percentage') {
      const parts = [
        `Payé : ${this.greenPercent.toFixed(1)}%`
      ];
      
      if (this.redPercent > 0) {
        parts.push(`En retard : ${this.redPercent.toFixed(1)}%`);
      }
      
      if (this.bluePercent > 0) {
        parts.push(`Restants : ${this.bluePercent.toFixed(1)}%`);
      }
      
      return parts.join(' | ');
    } else {
      const parts = [
        `Payés : ${this.data.greenMonths} mois`
      ];
      
      if (this.data.redMonths > 0) {
        parts.push(`En retard : ${this.data.redMonths} mois`);
      }
      
      if (this.data.blueMonths > 0) {
        parts.push(`Restants : ${this.data.blueMonths} mois`);
      }
      
      return parts.join(' | ');
    }
  }
}
