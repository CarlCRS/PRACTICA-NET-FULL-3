using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI_Carlos_Chica_20261109.Model;

namespace WebAPI_Carlos_Chica_20261109.Repository
{
    public class CatalogoRepository
    {
        private readonly IDbConnection _conn;

        public CatalogoRepository(IDbConnection conn) => _conn = conn;

        // -------------------------------------------------------------
        // COLABORADORES
        // -------------------------------------------------------------
        public async Task<List<Colaborador>> ObtenerColaboradoresAsync()
        {
            var sql = "SELECT Id, Cedula, Nombre, Correo FROM dbo.colaboradores ORDER BY Nombre";
            var lista = await _conn.QueryAsync<Colaborador>(sql);
            return lista.AsList();
        }

        public async Task<Colaborador?> ObtenerColaboradorAsync(int id)
        {
            var sql = "SELECT Id, Cedula, Nombre, Correo FROM dbo.colaboradores WHERE Id = @id";
            return await _conn.QueryFirstOrDefaultAsync<Colaborador>(sql, new { id });
        }

        public async Task<int> CrearColaboradorAsync(Colaborador c)
        {
            var sql = "INSERT INTO dbo.colaboradores (Cedula, Nombre, Correo) VALUES (@Cedula, @Nombre, @Correo); SELECT SCOPE_IDENTITY();";
            var result = await _conn.ExecuteScalarAsync(sql, c);
            return Convert.ToInt32(result);
        }

        public async Task<bool> ActualizarColaboradorAsync(int id, Colaborador c)
        {
            var sql = "UPDATE dbo.colaboradores SET Cedula = @Cedula, Nombre = @Nombre, Correo = @Correo WHERE Id = @id";
            var rows = await _conn.ExecuteAsync(sql, new { c.Cedula, c.Nombre, c.Correo, id });
            return rows > 0;
        }

        public async Task<bool> EliminarColaboradorAsync(int id)
        {
            var sql = "DELETE FROM dbo.colaboradores WHERE Id = @id";
            var rows = await _conn.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        // -------------------------------------------------------------
        // DEPARTAMENTOS
        // -------------------------------------------------------------
        public async Task<List<Departamento>> ObtenerDepartamentosAsync()
        {
            var sql = "SELECT Id, Nombre, PresupuestoAnual, ResponsableId, SuplenteId FROM dbo.departamentos ORDER BY Nombre";
            var lista = await _conn.QueryAsync<Departamento>(sql);
            return lista.AsList();
        }

        public async Task<int> CrearDepartamentoAsync(Departamento d)
        {
            var sql = "INSERT INTO dbo.departamentos (Nombre, PresupuestoAnual, ResponsableId, SuplenteId) VALUES (@Nombre, @PresupuestoAnual, @ResponsableId, @SuplenteId); SELECT SCOPE_IDENTITY();";
            var result = await _conn.ExecuteScalarAsync(sql, d);
            return Convert.ToInt32(result);
        }

        public async Task<bool> ActualizarDepartamentoAsync(int id, Departamento d)
        {
            var sql = "UPDATE dbo.departamentos SET Nombre = @Nombre, PresupuestoAnual = @PresupuestoAnual, ResponsableId = @ResponsableId, SuplenteId = @SuplenteId WHERE Id = @id";
            var rows = await _conn.ExecuteAsync(sql, new { d.Nombre, d.PresupuestoAnual, d.ResponsableId, d.SuplenteId, id });
            return rows > 0;
        }

        public async Task<bool> EliminarDepartamentoAsync(int id)
        {
            var sql = "DELETE FROM dbo.departamentos WHERE Id = @id";
            var rows = await _conn.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        // -------------------------------------------------------------
        // PROVEEDORES
        // -------------------------------------------------------------
        public async Task<List<Proveedor>> ObtenerProveedoresAsync()
        {
            var sql = "SELECT Id, Ruc, RazonSocial, Contacto FROM dbo.proveedores ORDER BY RazonSocial";
            var lista = await _conn.QueryAsync<Proveedor>(sql);
            return lista.AsList();
        }

        public async Task<int> CrearProveedorAsync(Proveedor p)
        {
            var sql = "INSERT INTO dbo.proveedores (Ruc, RazonSocial, Contacto) VALUES (@Ruc, @RazonSocial, @Contacto); SELECT SCOPE_IDENTITY();";
            var result = await _conn.ExecuteScalarAsync(sql, p);
            return Convert.ToInt32(result);
        }

        // -------------------------------------------------------------
        // REVISORES DESIGNADOS (Finanzas / Gerencia)
        // -------------------------------------------------------------
        public async Task<List<ConfigRevisor>> ObtenerConfigRevisoresAsync()
        {
            var sql = "SELECT Id, Rol, ColaboradorId, SuplenteId FROM dbo.config_revisor ORDER BY Rol";
            var lista = await _conn.QueryAsync<ConfigRevisor>(sql);
            return lista.AsList();
        }

        public async Task<ConfigRevisor?> ObtenerConfigRevisorPorRolAsync(string rol)
        {
            var sql = "SELECT Id, Rol, ColaboradorId, SuplenteId FROM dbo.config_revisor WHERE Rol = @rol";
            return await _conn.QueryFirstOrDefaultAsync<ConfigRevisor>(sql, new { rol });
        }
    }
}
