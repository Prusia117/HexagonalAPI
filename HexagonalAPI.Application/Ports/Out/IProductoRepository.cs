using System.Collections.Generic;
using HexagonalAPI.Domain.Productos;

namespace HexagonalAPI.Application.Ports.Out
{
    /// <summary>
    /// Puerto de salida: persistencia de productos.
    /// El Id es string porque la regla de negocio lo define como PROD-XX-xxxx-YY.
    /// </summary>
    public interface IProductoRepository
    {
        List<Producto> ObtenerTodos();

        Producto? ObtenerPorId(string id);

        List<Producto> Buscar(string termino);

        /// <summary>Inserta y devuelve el Id. Lanza <see cref="ProductoDuplicadoException"/> si el Id ya existe.</summary>
        string Insertar(Producto producto);

        bool Actualizar(Producto producto);

        bool Eliminar(string id);
    }
}
