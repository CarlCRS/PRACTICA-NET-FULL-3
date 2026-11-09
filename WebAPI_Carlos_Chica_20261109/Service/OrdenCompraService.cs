using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI_Carlos_Chica_20261109.Dtos;
using WebAPI_Carlos_Chica_20261109.Model;

namespace WebAPI_Carlos_Chica_20261109.Service
{
    public class OrdenCompraService
    {
        private readonly IDbConnection _conn;

        public OrdenCompraService(IDbConnection conn) => _conn = conn;

        public async Task<List<OrdenCompraListDTO>> ListarOrdenesAsync()
        {
            var sql = @"
                SELECT O.id, O.solicitud_id, P.razon_social AS ProveedorRazonSocial, O.fecha_emision, O.total
                FROM dbo.ordenes_compra O
                JOIN dbo.proveedores P ON P.Id = O.proveedor_id
                ORDER BY O.id DESC";
            var lista = await _conn.QueryAsync<OrdenCompraListDTO>(sql);
            return lista.AsList();
        }

        public async Task<OrdenCompraDetalleDTOFull?> ObtenerOrdenAsync(int id)
        {
            var sql = "SELECT id, solicitud_id, proveedor_id, fecha_emision, total FROM dbo.ordenes_compra WHERE id = @id";
            var orden = await _conn.QueryFirstOrDefaultAsync<OrdenCompra>(sql, new { id });
            if (orden == null) return null;

            var detalles = await ObtenerDetallesAsync(orden.Id);
            var sqlProv = "SELECT razon_social FROM dbo.proveedores WHERE Id = @id";
            string proveedor = await _conn.ExecuteScalarAsync<string>(sqlProv, new { id = orden.ProveedorId }) ?? string.Empty;

            return new OrdenCompraDetalleDTOFull(orden.Id, orden.SolicitudId, orden.ProveedorId,
                proveedor, orden.FechaEmision, orden.Total, detalles);
        }

        public async Task<int?> CrearOrdenAsync(int solicitudId, int proveedorId,
            List<OrdenCompraDetalleCreateDTO> detalles)
        {
            var sqlSol = "SELECT estado FROM dbo.solicitudes WHERE Id = @id";
            string? estado = await _conn.ExecuteScalarAsync<string?>(sqlSol, new { id = solicitudId });
            if (estado != "APROBADA") return null;

            int versionId = await ObtenerVersionVigenteIdAsync(solicitudId);
            if (versionId == 0) return null;

            foreach (var d in detalles)
            {
                var sqlAut = "SELECT ISNULL(SUM(cantidad), 0) FROM dbo.solicitud_detalles WHERE solicitud_version_id = @version AND descripcion = @descripcion";
                decimal autorizado = await _conn.ExecuteScalarAsync<decimal>(sqlAut, new { version = versionId, descripcion = d.Descripcion });

                var sqlOrd = "SELECT ISNULL(SUM(OD.cantidad), 0) FROM dbo.orden_compra_detalles OD JOIN dbo.ordenes_compra O ON O.id = OD.orden_compra_id WHERE O.solicitud_id = @solicitud AND OD.descripcion = @descripcion";
                decimal yaOrdenada = await _conn.ExecuteScalarAsync<decimal>(sqlOrd, new { solicitud = solicitudId, descripcion = d.Descripcion });

                if (yaOrdenada + d.Cantidad > autorizado) return null;
            }

            decimal total = 0;
            foreach (var d in detalles)
                total += d.Cantidad * d.PrecioUnitario;

            _conn.Open();
            using var transaction = _conn.BeginTransaction();
            try
            {
                var sqlIns = "INSERT INTO dbo.ordenes_compra (solicitud_id, proveedor_id, fecha_emision, total) OUTPUT INSERTED.id VALUES (@solicitud, @proveedor, GETDATE(), @total)";
                int ordenId = await _conn.ExecuteScalarAsync<int>(sqlIns, new { solicitud = solicitudId, proveedor = proveedorId, total }, transaction);

                foreach (var d in detalles)
                {
                    var sqlDet = "INSERT INTO dbo.orden_compra_detalles (orden_compra_id, descripcion, cantidad, precio_unitario) VALUES (@orden, @descripcion, @cantidad, @precio)";
                    await _conn.ExecuteAsync(sqlDet, new { orden = ordenId, descripcion = d.Descripcion, cantidad = d.Cantidad, precio = d.PrecioUnitario }, transaction);
                }

                transaction.Commit();
                return ordenId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task<int> ObtenerVersionVigenteIdAsync(int solicitudId)
        {
            var sql = "SELECT Id FROM dbo.solicitud_versiones WHERE solicitud_id = @id AND es_vigente = 1";
            var result = await _conn.ExecuteScalarAsync(sql, new { id = solicitudId });
            return result == null ? 0 : Convert.ToInt32(result);
        }

        private async Task<List<OrdenCompraDetalleDto>> ObtenerDetallesAsync(int ordenId)
        {
            var sql = "SELECT Id, descripcion, cantidad, precio_unitario FROM dbo.orden_compra_detalles WHERE orden_compra_id = @id";
            var lista = await _conn.QueryAsync<OrdenCompraDetalleDto>(sql, new { id = ordenId });
            return lista.AsList();
        }
    }
}
