namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class Proveedor
    {
        public int Id { get; set; }
        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? Contacto { get; set; }
    }
    public record ProveedorCreateDTO(string Ruc, string RazonSocial, string? Contacto);
    public record ProveedorResponseDTO(int Id, string Ruc, string RazonSocial, string? Contacto);
}
