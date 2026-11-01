using System;
using System.Threading;
using System.Threading.Tasks;
using HexagonalAPI.Domain.Productos;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace HexagonalAPI.Api.Middleware
{
    /// <summary>Traduce las excepciones del dominio a respuestas HTTP 400 (ProblemDetails).</summary>
    public sealed class ManejadorExcepciones : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext contexto,
            Exception excepcion,
            CancellationToken cancellationToken)
        {
            if (!(excepcion is ReglaDeNegocioException))
            {
                return false;
            }

            ProblemDetails detalle = new ProblemDetails();
            detalle.Status = StatusCodes.Status400BadRequest;
            detalle.Title = "Regla de negocio violada";
            detalle.Detail = excepcion.Message;

            contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
            await contexto.Response.WriteAsJsonAsync(detalle, cancellationToken);
            return true;
        }
    }
}
