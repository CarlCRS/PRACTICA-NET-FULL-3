import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SolicitudDetalleInfo {
  id: number;
  solicitanteId: number;
  solicitanteNombre: string;
  departamentoId: number;
  departamentoNombre: string;
  estado: string;
  versionVigente: number;
  detalles: DetalleDto[];
  etapas: EtapaDto[];
  decisiones: DecisionDto[];
}

export interface DetalleDto {
  id: number;
  descripcion: string;
  cantidad: number;
  precioUnitario: number;
}

export interface EtapaDto {
  orden: number;
  etapa: string;
  revisorNombre: string;
}

export interface DecisionDto {
  ordenEtapa: number;
  etapa: string;
  revisorId: number;
  revisorNombre?: string;
  decision: string;
  comentario?: string;
  fechaDecision?: string;
}

export interface SolicitudResumenDTO {
  id: number;
  estado: string;
  versionVigente: number;
  totalSolicitado: number;
  justificacion?: string;
  decisiones: DecisionDto[];
  revisorPendienteNombre?: string;
  revisorPendienteCorreo?: string;
}

@Injectable({ providedIn: 'root' })
export class SolicitudService {
  private readonly apiUrl = 'http://localhost:5267/api/solicitudes';

  constructor(private http: HttpClient) {}

  obtenerSolicitud(id: number): Observable<SolicitudDetalleInfo> {
    return this.http.get<SolicitudDetalleInfo>(`${this.apiUrl}/${id}`);
  }

  listarSolicitudes(): Observable<SolicitudResumenDTO[]> {
    return this.http.get<SolicitudResumenDTO[]>(this.apiUrl);
  }
}
