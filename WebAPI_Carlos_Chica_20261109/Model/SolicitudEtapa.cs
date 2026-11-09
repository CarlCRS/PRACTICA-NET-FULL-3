namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class SolicitudEtapa
    {
        public int Id { get; set; }
        public int SolicitudVersionId { get; set; }
        public int Orden { get; set; }
        public string Etapa { get; set; } = string.Empty;   
        public int RevisorId { get; set; }
    }

    public record SolicitudEtapaDTO(int Id, int Orden, string Etapa, int RevisorId, string? RevisorNombre);
}
