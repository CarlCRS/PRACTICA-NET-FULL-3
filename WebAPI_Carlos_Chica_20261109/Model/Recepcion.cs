namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class Recepcion
    {
        public int Id { get; set; }
        public int OrdenCompraId { get; set; }
        public DateTime FechaRecepcion { get; set; }
        public int ColaboradorId { get; set; }
        public string? Observaciones { get; set; }
    }

    public record RecepcionDTO(int Id, DateTime FechaRecepcion, int ColaboradorId,
        string? ColaboradorNombre, string? Observaciones);
    public record RecepcionCreateDTO(int OrdenCompraId, int ColaboradorId, string? Observaciones);
}
