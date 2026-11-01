using System;
using HexagonalAPI.Api.Middleware;
using HexagonalAPI.Infrastructure.Persistence.Sqlite;
using HexagonalAPI.Application.Ports.In;
using HexagonalAPI.Application.Ports.Out;
using HexagonalAPI.Application.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace HexagonalAPI.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Adaptadores de entrada (HTTP)
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<ManejadorExcepciones>();

            // Adaptadores de salida (SQLite) -> puertos de la aplicación
            string? connectionString = builder.Configuration.GetConnectionString("Productos");
            if (connectionString == null)
            {
                throw new InvalidOperationException("Falta la cadena de conexión 'Productos'.");
            }

            builder.Services.AddSingleton<SqliteConnectionFactory>(new SqliteConnectionFactory(connectionString));
            builder.Services.AddSingleton<InicializadorBaseDatos>();
            builder.Services.AddScoped<IProductoRepository, SqliteProductoRepository>();
            builder.Services.AddScoped<IProductoImagenRepository, SqliteProductoImagenRepository>();

            // Núcleo de la aplicación
            builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
            builder.Services.AddScoped<IProductoService, ProductoService>();

            WebApplication app = builder.Build();

            InicializadorBaseDatos inicializador = app.Services.GetRequiredService<InicializadorBaseDatos>();
            inicializador.Inicializar();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(delegate (SwaggerUIOptions opciones)
                {
                    opciones.SwaggerEndpoint("/openapi/v1.json", "HexagonalAPI v1");
                    opciones.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
