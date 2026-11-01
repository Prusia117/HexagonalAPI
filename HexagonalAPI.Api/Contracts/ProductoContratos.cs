using System;
using System.ComponentModel.DataAnnotations;
using HexagonalAPI.Domain.Productos;

namespace HexagonalAPI.Api.Contracts
{
    public class CrearProductoRequest
    {
        [Required]
        [StringLength(Producto.NombreMaxLongitud, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(Producto.DescripcionMaxLongitud)]
        public string? Descripcion { get; set; }

        [Range(0, 999999999.99)]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    public class ActualizarProductoRequest
    {
        [Required]
        [StringLength(Producto.NombreMaxLongitud, MinimumLength = 3)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(Producto.DescripcionMaxLongitud)]
        public string? Descripcion { get; set; }

        [Range(0, 999999999.99)]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    public class ProductoResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public DateTimeOffset FechaCreacion { get; set; }
        public bool TieneImagen { get; set; }
        public string? ImagenUrl { get; set; }

        public static ProductoResponse DesdeDominio(Producto producto)
        {
            ProductoResponse respuesta = new ProductoResponse();
            respuesta.Id = producto.Id;
            respuesta.Nombre = producto.Nombre;
            respuesta.Descripcion = producto.Descripcion;
            respuesta.Precio = producto.Precio;
            respuesta.Stock = producto.Stock;
            respuesta.FechaCreacion = producto.FechaCreacion;
            respuesta.TieneImagen = producto.TieneImagen;

            if (producto.TieneImagen)
            {
                respuesta.ImagenUrl = "/api/productos/" + producto.Id + "/imagen";
            }

            return respuesta;
        }
    }
}
