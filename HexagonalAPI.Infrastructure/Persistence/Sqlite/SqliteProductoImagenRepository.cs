using HexagonalAPI.Application.Ports.Out;
using HexagonalAPI.Domain.Productos;
using Microsoft.Data.Sqlite;

namespace HexagonalAPI.Infrastructure.Persistence.Sqlite
{
    public sealed class SqliteProductoImagenRepository : IProductoImagenRepository
    {
        private const int SqliteConstraint = 19;
        private const int SqliteConstraintForeignKey = 787;

        private readonly SqliteConnectionFactory _fabrica;

        public SqliteProductoImagenRepository(SqliteConnectionFactory fabrica)
        {
            this._fabrica = fabrica;
        }

        public bool Guardar(string productoId, ImagenProducto imagen)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        "INSERT INTO ProductoImagenes (ProductoId, TipoContenido, Datos) " +
                        "VALUES ($id, $tipo, $datos) " +
                        "ON CONFLICT(ProductoId) DO UPDATE SET " +
                        "TipoContenido = excluded.TipoContenido, Datos = excluded.Datos";
                    comando.Parameters.AddWithValue("$id", productoId);
                    comando.Parameters.AddWithValue("$tipo", imagen.TipoContenido);
                    comando.Parameters.AddWithValue("$datos", imagen.Datos);

                    try
                    {
                        comando.ExecuteNonQuery();
                        return true;
                    }
                    catch (SqliteException excepcion)
                    {
                        bool esRestriccion = excepcion.SqliteErrorCode == SqliteConstraint;
                        bool esLlaveForanea = excepcion.SqliteExtendedErrorCode == SqliteConstraintForeignKey;

                        if (esRestriccion && esLlaveForanea)
                        {
                            // El producto no existe.
                            return false;
                        }

                        throw;
                    }
                }
            }
        }

        public ImagenProducto? Obtener(string productoId)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        "SELECT TipoContenido, Datos FROM ProductoImagenes WHERE ProductoId = $id";
                    comando.Parameters.AddWithValue("$id", productoId);

                    using (SqliteDataReader lector = comando.ExecuteReader())
                    {
                        if (!lector.Read())
                        {
                            return null;
                        }

                        string tipoContenido = lector.GetString(0);
                        byte[] datos = (byte[])lector[1];
                        return ImagenProducto.Reconstruir(datos, tipoContenido);
                    }
                }
            }
        }

        public bool Eliminar(string productoId)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText = "DELETE FROM ProductoImagenes WHERE ProductoId = $id";
                    comando.Parameters.AddWithValue("$id", productoId);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
    }
}
