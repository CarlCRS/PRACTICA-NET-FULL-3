namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class SolicitudRevision
    {
        public int Id { get; set; }
        public int SolicitudVersionId { get; set; }
        public int? EtapaId { get; set; }
        public int OrdenEtapa { get; set; }
        public string EtapaNombre { get; set; } = string.Empty;
        public int RevisorId { get; set; }
        public string Decision { get; set; } = "PENDIENTE"; 
        public string? Comentario { get; set; }
        public DateTime? FechaDecision { get; set; }
    }

    public record SolicitudRevisionDTO(int Id, int OrdenEtapa, string EtapaNombre,
        int RevisorId, string? RevisorNombre, string Decision, string? Comentario, DateTime? FechaDecision);

    public record DecidirRevisionDTO(int RevisorId, string Decision, string? Comentario);
}
