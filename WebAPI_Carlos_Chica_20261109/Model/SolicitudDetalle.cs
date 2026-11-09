namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class SolicitudDetalle
    {
        public int Id { get; set; }
        public int SolicitudVersionId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe => Cantidad * PrecioUnitario;
    }

    public record SolicitudDetalleDTO(int Id, string Descripcion, decimal Cantidad,
        decimal PrecioUnitario, decimal Importe);

    public record SolicitudDetalleCreateDTO(string Descripcion, decimal Cantidad, decimal PrecioUnitario);
}
