namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class OrdenCompra
    {
        public int Id { get; set; }
        public int SolicitudId { get; set; }
        public int ProveedorId { get; set; }
        public DateTime FechaEmision { get; set; }
        public decimal Total { get; set; }
    }
    public record OrdenCompraCreateDTO(int SolicitudId, int ProveedorId, decimal Total,
        List<OrdenCompraDetalleCreateDTO> Detalles);

    public record OrdenCompraResponseDTO(int Id,int SolicitudId,string ProveedorRazonSocial,DateTime? FechaEmision,decimal Total);
}
