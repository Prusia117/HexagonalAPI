using System;

namespace HexagonalAPI.Domain.Productos
{
    /// <summary>Se viola una regla de negocio o una invariante del dominio.</summary>
    public class ReglaDeNegocioException : Exception
    {
        public ReglaDeNegocioException(string mensaje) : base(mensaje)
        {
        }
    }

    /// <summary>Ya existe un producto con el mismo Id (colisión del sufijo aleatorio).</summary>
    public class ProductoDuplicadoException : Exception
    {
        public ProductoDuplicadoException(string id)
            : base("Ya existe un producto con el Id '" + id + "'.")
        {
        }
    }
}
