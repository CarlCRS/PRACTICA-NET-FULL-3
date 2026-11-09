namespace WebAPI_Carlos_Chica_20261109.Dtos
{
    public record SolicitudDetalleInfo(
        int Id, int SolicitanteId, string SolicitanteNombre,
        int DepartamentoId, string DepartamentoNombre,
        string Estado, int VersionVigente,
        List<DetalleDto> Detalles, List<EtapaDto> Etapas, List<DecisionDto> Decisiones);

    public record DetalleDto(int Id, string Descripcion, decimal Cantidad, decimal PrecioUnitario);
    public record EtapaDto(int Orden, string Etapa, string RevisorNombre);
    public record DecisionDto(
        int OrdenEtapa, string Etapa, int RevisorId, string? RevisorNombre,
        string Decision, string? Comentario, DateTime? FechaDecision);

    public record SolicitudResumenDTO(
        int Id, string Estado, int VersionVigente, decimal TotalSolicitado,
        string? Justificacion, List<DecisionDto> Decisiones,
        string? RevisorPendienteNombre, string? RevisorPendienteCorreo);
}
