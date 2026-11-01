using System.Collections.Generic;
using System.IO;
using HexagonalAPI.Api.Contracts;
using HexagonalAPI.Application.Ports.In;
using HexagonalAPI.Domain.Productos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalAPI.Api.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public sealed class ProductosController : ControllerBase
    {
        private readonly IProductoService _servicio;

        public ProductosController(IProductoService servicio)
        {
            this._servicio = servicio;
        }

        [HttpGet]
        public ActionResult<List<ProductoResponse>> ObtenerTodos()
        {
            List<Producto> productos = this._servicio.ObtenerTodos();
            return Ok(ConvertirLista(productos));
        }

        /// <summary>Busca por Id, nombre o descripción (coincidencia parcial).</summary>
        [HttpGet("buscar")]
        public ActionResult<List<ProductoResponse>> Buscar([FromQuery] string termino)
        {
            List<Producto> productos = this._servicio.Buscar(termino);
            return Ok(ConvertirLista(productos));
        }

        [HttpGet("{id}")]
        public ActionResult<ProductoResponse> ObtenerPorId(string id)
        {
            Producto? producto = this._servicio.ObtenerPorId(id);
            if (producto == null)
            {
                return NotFound();
            }

            return Ok(ProductoResponse.DesdeDominio(producto));
        }

        /// <summary>El Id se genera solo: PROD-{1ª y 3ª letra del nombre}-{4 aleatorios}-{año 2 dígitos}.</summary>
        [HttpPost]
        public ActionResult<ProductoResponse> Crear(CrearProductoRequest request)
        {
            CrearProductoComando comando = new CrearProductoComando(
                request.Nombre, request.Descripcion, request.Precio, request.Stock);

            Producto producto = this._servicio.Crear(comando);

            return CreatedAtAction(
                nameof(ObtenerPorId),
                new { id = producto.Id },
                ProductoResponse.DesdeDominio(producto));
        }

        [HttpPut("{id}")]
        public ActionResult<ProductoResponse> Actualizar(string id, ActualizarProductoRequest request)
        {
            ActualizarProductoComando comando = new ActualizarProductoComando(
                request.Nombre, request.Descripcion, request.Precio, request.Stock);

            Producto? producto = this._servicio.Actualizar(id, comando);
            if (producto == null)
            {
                return NotFound();
            }

            return Ok(ProductoResponse.DesdeDominio(producto));
        }

        [HttpDelete("{id}")]
        public IActionResult Eliminar(string id)
        {
            bool eliminado = this._servicio.Eliminar(id);
            if (!eliminado)
            {
                return NotFound();
            }

            return NoContent();
        }

        // ---- Imagen ----

        /// <summary>Sube o reemplaza la imagen (PNG, JPEG, GIF o WebP, máx. 2 MB).</summary>
        [HttpPut("{id}/imagen")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(ImagenProducto.TamanoMaximoBytes + 65536)]
        public IActionResult SubirImagen(string id, IFormFile archivo)
        {
            if (archivo.Length > ImagenProducto.TamanoMaximoBytes)
            {
                throw new ReglaDeNegocioException("La imagen supera el máximo permitido.");
            }

            byte[] datos;
            using (MemoryStream memoria = new MemoryStream())
            {
                archivo.CopyTo(memoria);
                datos = memoria.ToArray();
            }

            bool guardada = this._servicio.GuardarImagen(id, datos);
            if (!guardada)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("{id}/imagen")]
        [Produces("image/png", "image/jpeg", "image/gif", "image/webp")]
        public IActionResult ObtenerImagen(string id)
        {
            ImagenProducto? imagen = this._servicio.ObtenerImagen(id);
            if (imagen == null)
            {
                return NotFound();
            }

            return File(imagen.Datos, imagen.TipoContenido);
        }

        [HttpDelete("{id}/imagen")]
        public IActionResult EliminarImagen(string id)
        {
            bool eliminada = this._servicio.EliminarImagen(id);
            if (!eliminada)
            {
                return NotFound();
            }

            return NoContent();
        }

        private static List<ProductoResponse> ConvertirLista(List<Producto> productos)
        {
            List<ProductoResponse> respuestas = new List<ProductoResponse>();
            foreach (Producto producto in productos)
            {
                respuestas.Add(ProductoResponse.DesdeDominio(producto));
            }

            return respuestas;
        }
    }
}
