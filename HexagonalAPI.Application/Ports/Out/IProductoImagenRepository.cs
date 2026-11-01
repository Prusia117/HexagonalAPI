using HexagonalAPI.Domain.Productos;

namespace HexagonalAPI.Application.Ports.Out
{
    /// <summary>Puerto de salida: almacenamiento de la imagen de cada producto.</summary>
    public interface IProductoImagenRepository
    {
        /// <summary>Guarda o reemplaza la imagen. Devuelve false si el producto no existe.</summary>
        bool Guardar(string productoId, ImagenProducto imagen);

        ImagenProducto? Obtener(string productoId);

        bool Eliminar(string productoId);
    }
}
