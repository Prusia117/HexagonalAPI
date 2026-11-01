using System.Collections.Generic;
using HexagonalAPI.Domain.Productos;

namespace HexagonalAPI.Application.Ports.In
{
    public sealed class CrearProductoComando
    {
        public string Nombre { get; private set; }
        public string? Descripcion { get; private set; }
        public decimal Precio { get; private set; }
        public int Stock { get; private set; }

        public CrearProductoComando(string nombre, string? descripcion, decimal precio, int stock)
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Precio = precio;
            this.Stock = stock;
        }
    }

    public sealed class ActualizarProductoComando
    {
        public string Nombre { get; private set; }
        public string? Descripcion { get; private set; }
        public decimal Precio { get; private set; }
        public int Stock { get; private set; }

        public ActualizarProductoComando(string nombre, string? descripcion, decimal precio, int stock)
        {
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Precio = precio;
            this.Stock = stock;
        }
    }

    /// <summary>Puerto de entrada: casos de uso de productos que consumen los adaptadores (HTTP, CLI, etc.).</summary>
    public interface IProductoService
    {
        List<Producto> ObtenerTodos();

        Producto? ObtenerPorId(string id);

        List<Producto> Buscar(string termino);

        Producto Crear(CrearProductoComando comando);

        Producto? Actualizar(string id, ActualizarProductoComando comando);

        bool Eliminar(string id);

        bool GuardarImagen(string id, byte[] datos);

        ImagenProducto? ObtenerImagen(string id);

        bool EliminarImagen(string id);
    }
}
