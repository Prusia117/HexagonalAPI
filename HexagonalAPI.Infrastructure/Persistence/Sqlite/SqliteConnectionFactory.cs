using Microsoft.Data.Sqlite;

namespace HexagonalAPI.Infrastructure.Persistence.Sqlite
{
    /// <summary>Conexión: entrega conexiones SQLite ya abiertas, con claves foráneas activas.</summary>
    public sealed class SqliteConnectionFactory
    {
        private readonly string _connectionString;

        public string DataSource { get; private set; }

        public SqliteConnectionFactory(string connectionString)
        {
            SqliteConnectionStringBuilder constructor = new SqliteConnectionStringBuilder(connectionString);
            constructor.ForeignKeys = true;

            this._connectionString = constructor.ToString();
            this.DataSource = constructor.DataSource;
        }

        public SqliteConnection Crear()
        {
            SqliteConnection conexion = new SqliteConnection(this._connectionString);
            conexion.Open();
            return conexion;
        }
    }
}
