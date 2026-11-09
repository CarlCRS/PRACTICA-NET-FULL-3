namespace WebAPI_Carlos_Chica_20261109.Dtos
{
    public record OrdenCompraListDTO(int Id, int SolicitudId, string ProveedorRazonSocial,
        DateTime? FechaEmision, decimal Total);

    public record OrdenCompraDetalleDTOFull(
        int Id, int SolicitudId, int ProveedorId, string ProveedorRazonSocial,
        DateTime FechaEmision, decimal Total, List<OrdenCompraDetalleDto> Detalles);

    public record OrdenCompraDetalleDto(int Id, string Descripcion, decimal Cantidad, decimal PrecioUnitario);
}
