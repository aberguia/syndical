import { Routes } from '@angular/router';

export const PARKING_ROUTES: Routes = [
  {
    path: 'cars',
    loadComponent: () => import('./cars/parking-cars.component').then(m => m.ParkingCarsComponent)
  },
  {
    path: 'places',
    loadComponent: () => import('./places/parking-places.component').then(m => m.ParkingPlacesComponent)
  },
  {
    path: '',
    redirectTo: 'cars',
    pathMatch: 'full'
  }
];
