using System;

namespace HexagonalAPI.Domain.Productos
{
    public sealed class Producto
    {
        public const int NombreMaxLongitud = 100;
        public const int DescripcionMaxLongitud = 500;

        public string Id { get; private set; }
        public string Nombre { get; private set; }
        public string? Descripcion { get; private set; }
        public decimal Precio { get; private set; }
        public int Stock { get; private set; }
        public DateTimeOffset FechaCreacion { get; private set; }

        /// <summary>Lo llena el repositorio al leer; no forma parte de las invariantes.</summary>
        public bool TieneImagen { get; private set; }

        private Producto(
            string id,
            string nombre,
            string? descripcion,
            decimal precio,
            int stock,
            DateTimeOffset fechaCreacion,
            bool tieneImagen)
        {
            this.Id = id;
            this.Nombre = nombre;
            this.Descripcion = descripcion;
            this.Precio = precio;
            this.Stock = stock;
            this.FechaCreacion = fechaCreacion;
            this.TieneImagen = tieneImagen;
        }

        /// <summary>Crea un producto nuevo y le asigna su Id según la regla de negocio.</summary>
        public static Producto Crear(string nombre, string? descripcion, decimal precio, int stock, TimeProvider reloj)
        {
            string nombreValido = ValidarNombre(nombre);
            string? descripcionValida = ValidarDescripcion(descripcion);
            ValidarPrecio(precio);
            ValidarStock(stock);

            string id = GeneradorIdProducto.Generar(nombreValido, reloj);
            DateTimeOffset fechaCreacion = reloj.GetUtcNow();

            return new Producto(id, nombreValido, descripcionValida, precio, stock, fechaCreacion, false);
        }

        /// <summary>Rehidrata un producto ya persistido (sin regenerar el Id).</summary>
        public static Producto Reconstruir(
            string id,
            string nombre,
            string? descripcion,
            decimal precio,
            int stock,
            DateTimeOffset fechaCreacion,
            bool tieneImagen)
        {
            return new Producto(id, nombre, descripcion, precio, stock, fechaCreacion, tieneImagen);
        }

        /// <summary>El Id no cambia al editar el nombre: es un identificador, no un derivado vivo.</summary>
        public void Actualizar(string nombre, string? descripcion, decimal precio, int stock)
        {
            string nombreValido = ValidarNombre(nombre);
            string? descripcionValida = ValidarDescripcion(descripcion);
            ValidarPrecio(precio);
            ValidarStock(stock);

            this.Nombre = nombreValido;
            this.Descripcion = descripcionValida;
            this.Precio = precio;
            this.Stock = stock;
        }

        private static string ValidarNombre(string nombre)
        {
            string nombreLimpio = string.Empty;
            if (nombre != null)
            {
                nombreLimpio = nombre.Trim();
            }

            if (nombreLimpio.Length == 0 || nombreLimpio.Length > NombreMaxLongitud)
            {
                throw new ReglaDeNegocioException(
                    "El nombre es obligatorio y admite hasta " + NombreMaxLongitud + " caracteres.");
            }

            return nombreLimpio;
        }

        private static string? ValidarDescripcion(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return null;
            }

            string descripcionLimpia = descripcion.Trim();
            if (descripcionLimpia.Length > DescripcionMaxLongitud)
            {
                throw new ReglaDeNegocioException(
                    "La descripción admite hasta " + DescripcionMaxLongitud + " caracteres.");
            }

            return descripcionLimpia;
        }

        private static void ValidarPrecio(decimal precio)
        {
            if (precio < 0)
            {
                throw new ReglaDeNegocioException("El precio no puede ser negativo.");
            }
        }

        private static void ValidarStock(int stock)
        {
            if (stock < 0)
            {
                throw new ReglaDeNegocioException("El stock no puede ser negativo.");
            }
        }
    }
}
