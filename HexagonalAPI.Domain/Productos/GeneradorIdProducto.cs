using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace HexagonalAPI.Domain.Productos
{
    /// <summary>
    /// Regla de negocio: PROD-{1ª y 3ª letra del nombre}-{4 caracteres a-zA-Z0-9 aleatorios}-{últimos 2 dígitos del año}.
    /// Ejemplo: "Laptop" => PROD-LP-aB3x-26
    /// </summary>
    public static class GeneradorIdProducto
    {
        private const string Alfabeto = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int LongitudAleatoria = 4;

        public static string Generar(string nombre, TimeProvider reloj)
        {
            string letras = ExtraerLetras(nombre);
            if (letras.Length < 3)
            {
                throw new ReglaDeNegocioException(
                    "El nombre del producto debe tener al menos 3 letras para poder generar su Id.");
            }

            string iniciales = letras[0].ToString() + letras[2].ToString();

            char[] caracteresAleatorios = new char[LongitudAleatoria];
            for (int i = 0; i < LongitudAleatoria; i++)
            {
                int posicion = RandomNumberGenerator.GetInt32(Alfabeto.Length);
                caracteresAleatorios[i] = Alfabeto[posicion];
            }
            string aleatorio = new string(caracteresAleatorios);

            int anioCompleto = reloj.GetUtcNow().Year;
            string anio = (anioCompleto % 100).ToString("D2", CultureInfo.InvariantCulture);

            return "PROD-" + iniciales + "-" + aleatorio + "-" + anio;
        }

        /// <summary>Solo letras A-Z en mayúscula; los acentos se descomponen (Á => A, ñ => N) y lo demás se ignora.</summary>
        private static string ExtraerLetras(string nombre)
        {
            string descompuesto = nombre.Normalize(NormalizationForm.FormD);
            StringBuilder constructor = new StringBuilder();

            foreach (char caracter in descompuesto)
            {
                bool esMayuscula = caracter >= 'A' && caracter <= 'Z';
                bool esMinuscula = caracter >= 'a' && caracter <= 'z';
                if (esMayuscula || esMinuscula)
                {
                    constructor.Append(char.ToUpperInvariant(caracter));
                }
            }

            return constructor.ToString();
        }
    }
}
