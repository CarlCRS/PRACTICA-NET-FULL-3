namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class OrdenCompraDetalle
    {
        public int Id { get; set; }
        public int OrdenCompraId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe => Cantidad * PrecioUnitario;
    }

    public record OrdenCompraDetalleDTO(int Id, string Descripcion, decimal Cantidad,
        decimal PrecioUnitario, decimal Importe);
    public record OrdenCompraDetalleCreateDTO(string Descripcion, decimal Cantidad, decimal PrecioUnitario);
}
