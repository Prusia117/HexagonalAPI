using System;
using System.Collections.Generic;
using System.Globalization;
using HexagonalAPI.Application.Ports.Out;
using HexagonalAPI.Domain.Productos;
using Microsoft.Data.Sqlite;

namespace HexagonalAPI.Infrastructure.Persistence.Sqlite
{
    public sealed class SqliteProductoRepository : IProductoRepository
    {
        private const int SqliteConstraint = 19;
        private const int SqliteConstraintPrimaryKey = 1555;
        private const int SqliteConstraintUnique = 2067;

        // Nunca se lee el BLOB de la imagen en los listados: solo si existe.
        private const string SelectProducto =
            "SELECT p.Id, p.Nombre, p.Descripcion, p.Precio, p.Stock, p.FechaCreacion, " +
            "       EXISTS (SELECT 1 FROM ProductoImagenes i WHERE i.ProductoId = p.Id) AS TieneImagen " +
            "FROM Productos p ";

        private readonly SqliteConnectionFactory _fabrica;

        public SqliteProductoRepository(SqliteConnectionFactory fabrica)
        {
            this._fabrica = fabrica;
        }

        public List<Producto> ObtenerTodos()
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText = SelectProducto + "ORDER BY p.FechaCreacion, p.Id";
                    return LeerProductos(comando);
                }
            }
        }

        public Producto? ObtenerPorId(string id)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText = SelectProducto + "WHERE p.Id = $id";
                    comando.Parameters.AddWithValue("$id", id);

                    List<Producto> encontrados = LeerProductos(comando);
                    if (encontrados.Count == 0)
                    {
                        return null;
                    }

                    return encontrados[0];
                }
            }
        }

        public List<Producto> Buscar(string termino)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        SelectProducto +
                        "WHERE p.Id LIKE $patron ESCAPE '\\' " +
                        "   OR p.Nombre LIKE $patron ESCAPE '\\' " +
                        "   OR p.Descripcion LIKE $patron ESCAPE '\\' " +
                        "ORDER BY p.FechaCreacion, p.Id";
                    comando.Parameters.AddWithValue("$patron", "%" + EscaparLike(termino) + "%");
                    return LeerProductos(comando);
                }
            }
        }

        public string Insertar(Producto producto)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        "INSERT INTO Productos (Id, Nombre, Descripcion, Precio, Stock, FechaCreacion) " +
                        "VALUES ($id, $nombre, $descripcion, $precio, $stock, $fecha)";
                    AgregarParametros(comando, producto);
                    comando.Parameters.AddWithValue(
                        "$fecha", producto.FechaCreacion.ToString("O", CultureInfo.InvariantCulture));

                    try
                    {
                        comando.ExecuteNonQuery();
                    }
                    catch (SqliteException excepcion)
                    {
                        bool esRestriccion = excepcion.SqliteErrorCode == SqliteConstraint;
                        bool esLlaveDuplicada =
                            excepcion.SqliteExtendedErrorCode == SqliteConstraintPrimaryKey ||
                            excepcion.SqliteExtendedErrorCode == SqliteConstraintUnique;

                        if (esRestriccion && esLlaveDuplicada)
                        {
                            throw new ProductoDuplicadoException(producto.Id);
                        }

                        throw;
                    }

                    return producto.Id;
                }
            }
        }

        public bool Actualizar(Producto producto)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    comando.CommandText =
                        "UPDATE Productos " +
                        "SET Nombre = $nombre, Descripcion = $descripcion, Precio = $precio, Stock = $stock " +
                        "WHERE Id = $id";
                    AgregarParametros(comando, producto);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        public bool Eliminar(string id)
        {
            using (SqliteConnection conexion = this._fabrica.Crear())
            {
                using (SqliteCommand comando = conexion.CreateCommand())
                {
                    // La imagen se borra en cascada (ON DELETE CASCADE).
                    comando.CommandText = "DELETE FROM Productos WHERE Id = $id";
                    comando.Parameters.AddWithValue("$id", id);

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }

        private static void AgregarParametros(SqliteCommand comando, Producto producto)
        {
            object descripcion = DBNull.Value;
            if (producto.Descripcion != null)
            {
                descripcion = producto.Descripcion;
            }

            comando.Parameters.AddWithValue("$id", producto.Id);
            comando.Parameters.AddWithValue("$nombre", producto.Nombre);
            comando.Parameters.AddWithValue("$descripcion", descripcion);
            comando.Parameters.AddWithValue("$precio", producto.Precio.ToString(CultureInfo.InvariantCulture));
            comando.Parameters.AddWithValue("$stock", producto.Stock);
        }

        private static List<Producto> LeerProductos(SqliteCommand comando)
        {
            List<Producto> productos = new List<Producto>();

            using (SqliteDataReader lector = comando.ExecuteReader())
            {
                while (lector.Read())
                {
                    string? descripcion = null;
                    if (!lector.IsDBNull(2))
                    {
                        descripcion = lector.GetString(2);
                    }

                    Producto producto = Producto.Reconstruir(
                        lector.GetString(0),
                        lector.GetString(1),
                        descripcion,
                        decimal.Parse(lector.GetString(3), CultureInfo.InvariantCulture),
                        lector.GetInt32(4),
                        DateTimeOffset.Parse(lector.GetString(5), CultureInfo.InvariantCulture),
                        lector.GetInt64(6) == 1);

                    productos.Add(producto);
                }
            }

            return productos;
        }

        private static string EscaparLike(string termino)
        {
            string escapado = termino.Replace("\\", "\\\\");
            escapado = escapado.Replace("%", "\\%");
            escapado = escapado.Replace("_", "\\_");
            return escapado;
        }
    }
}
