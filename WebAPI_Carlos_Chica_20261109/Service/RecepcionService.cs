using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI_Carlos_Chica_20261109.Dtos;

namespace WebAPI_Carlos_Chica_20261109.Service
{
    public class RecepcionService
    {
        private readonly IDbConnection _conn;

        public RecepcionService(IDbConnection conn) => _conn = conn;

        public async Task<List<RecepcionDto>> ListarRecepcionesAsync(int ordenCompraId)
        {
            var sql = @"
                SELECT R.Id, R.fecha_recepcion, C.nombre AS ColaboradorNombre, R.observaciones
                FROM dbo.recepciones R
                JOIN dbo.colaboradores C ON C.Id = R.colaborador_id
                WHERE R.orden_compra_id = @orden
                ORDER BY R.Id DESC";
            var lista = await _conn.QueryAsync<RecepcionDto>(sql, new { orden = ordenCompraId });
            return lista.AsList();
        }

        public async Task<int?> CrearRecepcionAsync(int ordenCompraId, int colaboradorId, string? observaciones)
        {
            var sqlEx = "SELECT COUNT(*) FROM dbo.ordenes_compra WHERE id = @id";
            bool existe = await _conn.ExecuteScalarAsync<bool>(sqlEx, new { id = ordenCompraId });
            if (!existe) return null;

            var sqlIns = "INSERT INTO dbo.recepciones (orden_compra_id, fecha_recepcion, colaborador_id, observaciones) OUTPUT INSERTED.Id VALUES (@orden, GETDATE(), @colaborador, @observaciones)";
            int id = await _conn.ExecuteScalarAsync<int>(sqlIns, new { orden = ordenCompraId, colaborador = colaboradorId, observaciones });
            return id;
        }
    }
}
