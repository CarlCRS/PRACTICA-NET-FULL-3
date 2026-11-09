using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI_Carlos_Chica_20261109.Dtos;
using WebAPI_Carlos_Chica_20261109.Model;

namespace WebAPI_Carlos_Chica_20261109.Service
{
    public class SolicitudService
    {
        private readonly IDbConnection _conn;

        public SolicitudService(IDbConnection conn) => _conn = conn;

        public async Task<List<SolicitudResumenDTO>> ListarSolicitudesAsync()
        {
            var sql = @"
                SELECT s.Id, s.estado, s.version_vigente_num,
                       v.total_estimado AS TotalSolicitado, v.justificacion, v.Id AS VersionId
                FROM dbo.solicitudes s
                LEFT JOIN dbo.solicitud_versiones v
                  ON v.solicitud_id = s.Id AND v.es_vigente = 1
                ORDER BY s.Id DESC";

            var filas = await _conn.QueryAsync(sql);
            var lista = new List<SolicitudResumenDTO>();

            foreach (var fila in filas)
            {
                int id = fila.Id;
                string estado = fila.estado;
                int versionVigente = fila.version_vigente_num ?? 1;
                decimal total = fila.TotalSolicitado ?? 0;
                string? justificacion = fila.justificacion;
                int? versionId = fila.VersionId;

                var decisiones = await ObtenerDecisionesAsync(versionId);
                string? revisorPendiente = null;
                string? revisorPendienteCorreo = null;
                if (versionId.HasValue)
                {
                    var pendiente = await ObtenerRevisorPendienteAsync(versionId.Value);
                    revisorPendiente = pendiente.Nombre;
                    revisorPendienteCorreo = pendiente.Correo;
                }

                lista.Add(new SolicitudResumenDTO(
                    id, estado, versionVigente, total, justificacion,
                    decisiones, revisorPendiente, revisorPendienteCorreo));
            }
            return lista;
        }

        public async Task<SolicitudDetalleInfo?> ObtenerSolicitudAsync(int id)
        {
            var sql = "SELECT Id, solicitante_id, departamento_id, estado, version_vigente_num FROM dbo.solicitudes WHERE Id = @id";
            var solicitud = await _conn.QueryFirstOrDefaultAsync<Solicitud>(sql, new { id });
            if (solicitud == null) return null;

            int versionId = await ObtenerVersionVigenteIdAsync(solicitud.Id);
            var detalles = await ObtenerDetallesAsync(versionId);
            var etapas = await ObtenerEtapasConRevisorAsync(versionId);
            var decisiones = await ObtenerDecisionesAsync(versionId);

            return new SolicitudDetalleInfo(
                solicitud.Id, solicitud.SolicitanteId, await ObtenerNombreColaboradorAsync(solicitud.SolicitanteId) ?? string.Empty,
                solicitud.DepartamentoId, await ObtenerNombreDepartamentoAsync(solicitud.DepartamentoId) ?? string.Empty,
                solicitud.Estado, solicitud.VersionVigenteNum,
                detalles, etapas, decisiones);
        }

        public async Task<int> CrearSolicitudAsync(int solicitanteId, int departamentoId, string justificacion,
            List<SolicitudDetalleCreateDTO> detalles)
        {
            int solicitudId;
            _conn.Open();
            using var transaction = _conn.BeginTransaction();

            try
            {
                var sqlSol = "INSERT INTO dbo.solicitudes (solicitante_id, departamento_id, estado, version_vigente_num) OUTPUT INSERTED.Id VALUES (@solicitante, @departamento, 'BORRADOR', 1)";
                solicitudId = await _conn.ExecuteScalarAsync<int>(sqlSol, new { solicitante = solicitanteId, departamento = departamentoId }, transaction);

                decimal total = 0;
                foreach (var d in detalles)
                    total += d.Cantidad * d.PrecioUnitario;

                var sqlVer = "INSERT INTO dbo.solicitud_versiones (solicitud_id, version_num, fecha_creacion, justificacion, total_estimado, es_vigente) OUTPUT INSERTED.Id VALUES (@solicitud, 1, GETDATE(), @justificacion, @total, 1)";
                int versionId = await _conn.ExecuteScalarAsync<int>(sqlVer, new { solicitud = solicitudId, justificacion, total }, transaction);

                foreach (var d in detalles)
                {
                    var sqlDet = "INSERT INTO dbo.solicitud_detalles (solicitud_version_id, descripcion, cantidad, precio_unitario) VALUES (@version, @descripcion, @cantidad, @precio)";
                    await _conn.ExecuteAsync(sqlDet, new { version = versionId, descripcion = d.Descripcion, cantidad = d.Cantidad, precio = d.PrecioUnitario }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return solicitudId;
        }

        public async Task<bool> EnviarRevisionAsync(int solicitudId, int solicitanteId)
        {
            var sqlEstado = "SELECT estado, departamento_id FROM dbo.solicitudes WHERE Id = @id";
            var datos = await _conn.QueryFirstOrDefaultAsync<(string estado, int departamento_id)>(sqlEstado, new { id = solicitudId });
            if (datos.estado != "BORRADOR") return false;

            int versionId = await ObtenerVersionVigenteIdAsync(solicitudId);
            decimal total = await ObtenerTotalVersionAsync(versionId);
            var etapas = DeterminarEtapas(total);

            _conn.Open();
            using var transaction = _conn.BeginTransaction();
            try
            {
                int orden = 1;
                foreach (var (nombreEtapa, _) in etapas)
                {
                    int revisorId = await ObtenerRevisorEtapaAsync(nombreEtapa, datos.departamento_id, solicitanteId);

                    var sqlEtapa = "INSERT INTO dbo.solicitud_etapas (solicitud_version_id, orden, etapa, revisor_id) OUTPUT INSERTED.Id VALUES (@version, @orden, @etapa, @revisor)";
                    int etapaId = await _conn.ExecuteScalarAsync<int>(sqlEtapa, new { version = versionId, orden, etapa = nombreEtapa, revisor = revisorId }, transaction);

                    var sqlRev = "INSERT INTO dbo.solicitud_revisiones (solicitud_version_id, etapa_id, orden_etapa, etapa_nombre, revisor_id, decision, comentario, fecha_decision) VALUES (@version, @etapaId, @orden, @etapa, @revisor, 'PENDIENTE', NULL, NULL)";
                    await _conn.ExecuteAsync(sqlRev, new { version = versionId, etapaId, orden, etapa = nombreEtapa, revisor = revisorId }, transaction);

                    orden++;
                }

                var sqlUpd = "UPDATE dbo.solicitudes SET estado = 'EN_REVISION' WHERE Id = @id";
                await _conn.ExecuteAsync(sqlUpd, new { id = solicitudId }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return true;
        }

        public async Task<bool> DecidirRevisionAsync(int revisionId, int revisorId, string decision, string? comentario)
        {
            var sqlRev = "SELECT solicitud_version_id, orden_etapa, decision, revisor_id FROM dbo.solicitud_revisiones WHERE Id = @id";
            var rev = await _conn.QueryFirstOrDefaultAsync<(int solicitud_version_id, int orden_etapa, string decision, int revisor_id)>(sqlRev, new { id = revisionId });
            if (rev.decision != "PENDIENTE") return false;
            if (rev.revisor_id != revisorId) return false;
            if ((decision == "RECHAZADO" || decision == "DEVUELTO") && string.IsNullOrWhiteSpace(comentario))
                return false;

            var sqlPend = "SELECT COUNT(*) FROM dbo.solicitud_revisiones WHERE solicitud_version_id = @version AND orden_etapa < @orden AND decision = 'PENDIENTE'";
            int pendientes = await _conn.ExecuteScalarAsync<int>(sqlPend, new { version = rev.solicitud_version_id, orden = rev.orden_etapa });
            if (pendientes > 0) return false;

            _conn.Open();
            using var transaction = _conn.BeginTransaction();
            try
            {
                var sqlUpd = "UPDATE dbo.solicitud_revisiones SET decision = @decision, comentario = @comentario, fecha_decision = GETDATE() WHERE Id = @id";
                await _conn.ExecuteAsync(sqlUpd, new { decision, comentario, id = revisionId }, transaction);

                if (decision == "RECHAZADO")
                {
                    var sqlSol = "UPDATE dbo.solicitudes SET estado = 'RECHAZADA' WHERE Id = (SELECT solicitud_id FROM dbo.solicitud_versiones WHERE Id = @version)";
                    await _conn.ExecuteAsync(sqlSol, new { version = rev.solicitud_version_id }, transaction);
                }
                else if (decision == "APROBADO")
                {
                    var sqlCount = "SELECT COUNT(*) FROM dbo.solicitud_revisiones WHERE solicitud_version_id = @version AND decision != 'APROBADO'";
                    int noAprobadas = await _conn.ExecuteScalarAsync<int>(sqlCount, new { version = rev.solicitud_version_id }, transaction);
                    if (noAprobadas == 0)
                    {
                        var sqlSol = "UPDATE dbo.solicitudes SET estado = 'APROBADA' WHERE Id = (SELECT solicitud_id FROM dbo.solicitud_versiones WHERE Id = @version)";
                        await _conn.ExecuteAsync(sqlSol, new { version = rev.solicitud_version_id }, transaction);
                    }
                }
                else if (decision == "DEVUELTO")
                {
                    var sqlSol = "UPDATE dbo.solicitudes SET estado = 'DEVUELTA' WHERE Id = (SELECT solicitud_id FROM dbo.solicitud_versiones WHERE Id = @version)";
                    await _conn.ExecuteAsync(sqlSol, new { version = rev.solicitud_version_id }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            return true;
        }

        public async Task<bool> CancelarSolicitudAsync(int solicitudId)
        {
            var sqlOrd = "SELECT COUNT(*) FROM dbo.ordenes_compra WHERE solicitud_id = @id";
            int ordenes = await _conn.ExecuteScalarAsync<int>(sqlOrd, new { id = solicitudId });
            if (ordenes > 0) return false;

            var sqlSol = "UPDATE dbo.solicitudes SET estado = 'CANCELADA' WHERE Id = @id";
            var rows = await _conn.ExecuteAsync(sqlSol, new { id = solicitudId });
            return rows > 0;
        }

        private async Task<int> ObtenerVersionVigenteIdAsync(int solicitudId)
        {
            var sql = "SELECT Id FROM dbo.solicitud_versiones WHERE solicitud_id = @id AND es_vigente = 1";
            var result = await _conn.ExecuteScalarAsync(sql, new { id = solicitudId });
            return result == null ? 0 : Convert.ToInt32(result);
        }

        private async Task<decimal> ObtenerTotalVersionAsync(int versionId)
        {
            var sql = "SELECT ISNULL(SUM(cantidad * precio_unitario), 0) FROM dbo.solicitud_detalles WHERE solicitud_version_id = @id";
            return await _conn.ExecuteScalarAsync<decimal>(sql, new { id = versionId });
        }

        private List<(string Nombre, int Orden)> DeterminarEtapas(decimal total)
        {
            var etapas = new List<(string Nombre, int Orden)>();
            if (total <= 500)
                etapas.Add(("Responsable", 1));
            else if (total <= 3000)
            {
                etapas.Add(("Responsable", 1));
                etapas.Add(("Finanzas", 2));
            }
            else
            {
                etapas.Add(("Responsable", 1));
                etapas.Add(("Finanzas", 2));
                etapas.Add(("Gerencia", 3));
            }
            return etapas;
        }

        private async Task<int> ObtenerRevisorEtapaAsync(string etapa, int departamentoId, int solicitanteId)
        {
            int revisorId;

            if (etapa == "Responsable")
            {
                var sql = "SELECT responsable_id, suplente_id FROM dbo.departamentos WHERE Id = @id";
                var depto = await _conn.QueryFirstOrDefaultAsync<(int responsable_id, int? suplente_id)>(sql, new { id = departamentoId });
                if (depto.responsable_id == solicitanteId && depto.suplente_id.HasValue)
                    revisorId = depto.suplente_id.Value;
                else
                    revisorId = depto.responsable_id;
            }
            else if (etapa == "Finanzas" || etapa == "Gerencia")
            {
                var sql = "SELECT colaborador_id, suplente_id FROM dbo.config_revisor WHERE rol = @rol";
                var cfg = await _conn.QueryFirstOrDefaultAsync<(int colaborador_id, int? suplente_id)>(sql, new { rol = etapa });
                if (cfg.colaborador_id == solicitanteId && cfg.suplente_id.HasValue)
                    revisorId = cfg.suplente_id.Value;
                else
                    revisorId = cfg.colaborador_id;
            }
            else
            {
                revisorId = solicitanteId;
            }
            return revisorId;
        }

        private async Task<List<DetalleDto>> ObtenerDetallesAsync(int versionId)
        {
            var sql = "SELECT Id, descripcion, cantidad, precio_unitario FROM dbo.solicitud_detalles WHERE solicitud_version_id = @id";
            var lista = await _conn.QueryAsync<DetalleDto>(sql, new { id = versionId });
            return lista.AsList();
        }

        private async Task<List<EtapaDto>> ObtenerEtapasConRevisorAsync(int versionId)
        {
            var sql = @"SELECT E.orden, E.etapa, C.nombre AS RevisorNombre
                        FROM dbo.solicitud_etapas E
                        JOIN dbo.colaboradores C ON C.Id = E.revisor_id
                        WHERE E.solicitud_version_id = @id ORDER BY E.orden";
            var lista = await _conn.QueryAsync<EtapaDto>(sql, new { id = versionId });
            return lista.AsList();
        }

        private async Task<List<DecisionDto>> ObtenerDecisionesAsync(int? versionId)
        {
            var lista = new List<DecisionDto>();
            if (!versionId.HasValue) return lista;

            var sql = @"SELECT R.orden_etapa, R.etapa_nombre, R.revisor_id, C.nombre AS RevisorNombre,
                               R.decision, R.comentario, R.fecha_decision
                        FROM dbo.solicitud_revisiones R
                        LEFT JOIN dbo.colaboradores C ON C.Id = R.revisor_id
                        WHERE R.solicitud_version_id = @id ORDER BY R.orden_etapa";
            var res = await _conn.QueryAsync<DecisionDto>(sql, new { id = versionId.Value });
            return res.AsList();
        }

        private async Task<(string? Nombre, string? Correo)> ObtenerRevisorPendienteAsync(int versionId)
        {
            var sql = @"SELECT TOP 1 C.nombre AS Nombre, C.correo AS Correo
                        FROM dbo.solicitud_revisiones R
                        JOIN dbo.colaboradores C ON C.Id = R.revisor_id
                        WHERE R.solicitud_version_id = @id AND R.decision = 'PENDIENTE'
                        ORDER BY R.orden_etapa";
            return await _conn.QueryFirstOrDefaultAsync<(string? Nombre, string? Correo)>(sql, new { id = versionId });
        }

        private async Task<string?> ObtenerNombreColaboradorAsync(int id)
        {
            var sql = "SELECT nombre FROM dbo.colaboradores WHERE Id = @id";
            return await _conn.ExecuteScalarAsync<string?>(sql, new { id });
        }

        private async Task<string?> ObtenerNombreDepartamentoAsync(int id)
        {
            var sql = "SELECT nombre FROM dbo.departamentos WHERE Id = @id";
            return await _conn.ExecuteScalarAsync<string?>(sql, new { id });
        }
    }
}
