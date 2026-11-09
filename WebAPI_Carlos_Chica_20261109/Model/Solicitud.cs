namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class Solicitud
    {
        public int Id { get; set; }
        public int SolicitanteId { get; set; }
        public int DepartamentoId { get; set; }
        public string Estado { get; set; } = "BORRADOR";
        public int VersionVigenteNum { get; set; } = 1;
    }

    public record SolicitudCreateDTO(int SolicitanteId, int DepartamentoId, string Justificacion);
}
