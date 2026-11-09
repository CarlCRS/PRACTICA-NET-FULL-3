using Microsoft.Data.SqlClient;
using System.Data.SqlClient;

namespace WebAPI_Carlos_Chica_20261109.Repository
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BaseConection")
                ?? throw new Exception("No se encontró la cadena de conexión 'BaseConection' en appsettings.json.");
        }

        public SqlConnection CrearConexion() => new SqlConnection(_connectionString);
    }
}
