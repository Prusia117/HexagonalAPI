using System.IO;
using Microsoft.Data.Sqlite;

namespace HexagonalAPI.Infrastructure.Persistence.Sqlite
{
    /// <summary>Crea el archivo y las tablas si no existen.</summary>
    public sealed class InicializadorBaseDatos
    {
        private readonly SqliteConnectionFactory _fabrica;

        public InicializadorBaseDatos(SqliteConnectionFactory fabrica)
        {
            this._fabrica = fabrica;
        }

        public void Inicializar()
        {
            string rutaCompleta = Path.GetFullPath(this._fabrica.DataSource);
            string? directorio = Path.GetDirectoryName(rutaCompleta);
            if (!string.IsNullOrEmpty(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        "CREATE TABLE IF NOT EXISTS Productos (" +
                        "    Id            TEXT NOT NULL PRIMARY KEY," +
                        "    Nombre        TEXT NOT NULL CHECK (length(Nombre) BETWEEN 1 AND 100)," +
                        "    Descripcion   TEXT NULL," +
                        "    Precio        TEXT NOT NULL," +
                        "    Stock         INTEGER NOT NULL CHECK (Stock >= 0)," +
                        "    FechaCreacion TEXT NOT NULL" +
                        ");" +
                        "CREATE TABLE IF NOT EXISTS ProductoImagenes (" +
                        "    ProductoId    TEXT NOT NULL PRIMARY KEY REFERENCES Productos(Id) ON DELETE CASCADE," +
                        "    TipoContenido TEXT NOT NULL," +
                        "    Datos         BLOB NOT NULL" +
                        ");";
                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}
