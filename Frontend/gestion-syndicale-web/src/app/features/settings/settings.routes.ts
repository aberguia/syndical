import { Routes } from '@angular/router';
import { BuildingsComponent } from './buildings/buildings.component';
import { ApartmentsComponent } from './apartments/apartments.component';
import { MembersComponent } from './members/members.component';
import { NotesComponent } from './notes/notes.component';

export const SETTINGS_ROUTES: Routes = [
  {
    path: 'buildings',
    component: BuildingsComponent
  },
  {
    path: 'apartments',
    component: ApartmentsComponent
  },
  {
    path: 'members',
    component: MembersComponent
  },
  {
    path: 'notes',
    component: NotesComponent
  },
  {
    path: '',
    redirectTo: 'buildings',
    pathMatch: 'full'
  }
];
