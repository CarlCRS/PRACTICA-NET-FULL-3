namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class SolicitudVersion
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public int VersionNum { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Justificacion { get; set; } = string.Empty;
        public decimal TotalEstimado { get; set; }
        public bool EsVigente { get; set; }
    }

    public record SolicitudVersionDTO(int Id, int VersionNum, DateTime FechaCreacion,
        string Justificacion, decimal TotalEstimado, bool EsVigente);
}
