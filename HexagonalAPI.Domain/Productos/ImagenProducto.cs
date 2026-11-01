namespace HexagonalAPI.Domain.Productos
{
    /// <summary>
    /// Imagen de un producto. El tipo se deduce de los primeros bytes del archivo
    /// (no se confía en el Content-Type que declare el cliente).
    /// </summary>
    public sealed class ImagenProducto
    {
        public const int TamanoMaximoBytes = 2 * 1024 * 1024;

        private static readonly byte[] FirmaPng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        private static readonly byte[] FirmaJpeg = new byte[] { 0xFF, 0xD8, 0xFF };
        private static readonly byte[] FirmaGif87 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }; // GIF87a
        private static readonly byte[] FirmaGif89 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }; // GIF89a
        private static readonly byte[] FirmaRiff = new byte[] { 0x52, 0x49, 0x46, 0x46 };               // RIFF
        private static readonly byte[] FirmaWebp = new byte[] { 0x57, 0x45, 0x42, 0x50 };               // WEBP

        public byte[] Datos { get; private set; }
        public string TipoContenido { get; private set; }

        private ImagenProducto(byte[] datos, string tipoContenido)
        {
            this.Datos = datos;
            this.TipoContenido = tipoContenido;
        }

        public static ImagenProducto Crear(byte[] datos)
        {
            if (datos.Length == 0)
            {
                throw new ReglaDeNegocioException("La imagen está vacía.");
            }

            if (datos.Length > TamanoMaximoBytes)
            {
                throw new ReglaDeNegocioException(
                    "La imagen supera el máximo de " + (TamanoMaximoBytes / (1024 * 1024)) + " MB.");
            }

            string? tipo = DetectarTipo(datos);
            if (tipo == null)
            {
                throw new ReglaDeNegocioException("Formato de imagen no soportado. Usa PNG, JPEG, GIF o WebP.");
            }

            return new ImagenProducto(datos, tipo);
        }

        /// <summary>Rehidrata una imagen ya validada y persistida.</summary>
        public static ImagenProducto Reconstruir(byte[] datos, string tipoContenido)
        {
            return new ImagenProducto(datos, tipoContenido);
        }

        private static string? DetectarTipo(byte[] datos)
        {
            if (EmpiezaCon(datos, 0, FirmaPng))
            {
                return "image/png";
            }

            if (EmpiezaCon(datos, 0, FirmaJpeg))
            {
                return "image/jpeg";
            }

            if (EmpiezaCon(datos, 0, FirmaGif87) || EmpiezaCon(datos, 0, FirmaGif89))
            {
                return "image/gif";
            }

            // WebP: "RIFF" + 4 bytes de tamaño + "WEBP"
            if (EmpiezaCon(datos, 0, FirmaRiff) && EmpiezaCon(datos, 8, FirmaWebp))
            {
                return "image/webp";
            }

            return null;
        }

        private static bool EmpiezaCon(byte[] datos, int desplazamiento, byte[] firma)
        {
            if (datos.Length < desplazamiento + firma.Length)
            {
                return false;
            }

            for (int i = 0; i < firma.Length; i++)
            {
                if (datos[desplazamiento + i] != firma[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
