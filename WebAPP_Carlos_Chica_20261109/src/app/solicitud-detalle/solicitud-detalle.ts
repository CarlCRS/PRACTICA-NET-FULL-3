import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SolicitudService, SolicitudDetalleInfo } from '../solicitud.service';

@Component({
  selector: 'app-solicitud-detalle',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './solicitud-detalle.html',
  styleUrl: './solicitud-detalle.css',
})
export class SolicitudDetalle {
  idSolicitud = '';
  solicitud?: SolicitudDetalleInfo;
  cargando = false;
  error = '';

  constructor(private solicitudService: SolicitudService) {}

  consultar() {
    const id = Number(this.idSolicitud);
    if (!id) {
      this.error = 'Ingresa un ID válido';
      return;
    }
    this.cargando = true;
    this.error = '';
    this.solicitud = undefined;

    this.solicitudService.obtenerSolicitud(id).subscribe({
      next: (data) => {
        this.solicitud = data;
        this.cargando = false;
      },
      error: () => {
        this.error = 'No se pudo cargar la solicitud';
        this.cargando = false;
      },
    });
  }
}
