namespace SistemaBiometricoPolicia.Models
{
    public class Estudiante
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; }
        public string Documento => NumeroDocumento; // Alias para compatibilidad
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Seccion { get; set; }
        public string RutaFoto { get; set; }
        public string FotoRuta => RutaFoto; // Alias para compatibilidad
    }
}