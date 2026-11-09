import { Routes } from '@angular/router';
import { SolicitudDetalle } from './solicitud-detalle/solicitud-detalle';

export const routes: Routes = [
  { path: 'soli', component: SolicitudDetalle },
  { path: '', redirectTo: '/soli', pathMatch: 'full' },
];
