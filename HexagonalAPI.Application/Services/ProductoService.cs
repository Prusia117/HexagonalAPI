using System;
using System.Collections.Generic;
using HexagonalAPI.Application.Ports.In;
using HexagonalAPI.Application.Ports.Out;
using HexagonalAPI.Domain.Productos;

namespace HexagonalAPI.Application.Services
{
    public sealed class ProductoService : IProductoService
    {
        // El sufijo aleatorio tiene 62^4 combinaciones por prefijo/año: una colisión es rara, pero posible.
        private const int MaxIntentosId = 5;

        private readonly IProductoRepository _productos;
        private readonly IProductoImagenRepository _imagenes;
        private readonly TimeProvider _reloj;

        public ProductoService(
            IProductoRepository productos,
            IProductoImagenRepository imagenes,
            TimeProvider reloj)
        {
            this._productos = productos;
            this._imagenes = imagenes;
            this._reloj = reloj;
        }

        public List<Producto> ObtenerTodos()
        {
            return this._productos.ObtenerTodos();
        }

        public Producto? ObtenerPorId(string id)
        {
            return this._productos.ObtenerPorId(id);
        }

        public List<Producto> Buscar(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
            {
                return this._productos.ObtenerTodos();
            }

            return this._productos.Buscar(termino.Trim());
        }

        public Producto Crear(CrearProductoComando comando)
        {
            for (int intento = 0; intento < MaxIntentosId; intento++)
            {
                Producto producto = Producto.Crear(
                    comando.Nombre, comando.Descripcion, comando.Precio, comando.Stock, this._reloj);

                try
                {
                    this._productos.Insertar(producto);
                    return producto;
                }
                catch (ProductoDuplicadoException)
                {
                    // Se genera un nuevo Id en la siguiente vuelta.
                }
            }

            throw new InvalidOperationException("No se pudo generar un Id único para el producto.");
        }

        public Producto? Actualizar(string id, ActualizarProductoComando comando)
        {
            Producto? producto = this._productos.ObtenerPorId(id);
            if (producto == null)
            {
                return null;
            }

            producto.Actualizar(comando.Nombre, comando.Descripcion, comando.Precio, comando.Stock);

            bool actualizado = this._productos.Actualizar(producto);
            if (!actualizado)
            {
                return null;
            }

            return producto;
        }

        public bool Eliminar(string id)
        {
            return this._productos.Eliminar(id);
        }

        public bool GuardarImagen(string id, byte[] datos)
        {
            ImagenProducto imagen = ImagenProducto.Crear(datos);
            return this._imagenes.Guardar(id, imagen);
        }

        public ImagenProducto? ObtenerImagen(string id)
        {
            return this._imagenes.Obtener(id);
        }

        public bool EliminarImagen(string id)
        {
            return this._imagenes.Eliminar(id);
        }
    }
}
