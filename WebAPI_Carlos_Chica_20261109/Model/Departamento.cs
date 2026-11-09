namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class Departamento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PresupuestoAnual { get; set; }
        public int? ResponsableId { get; set; }
        public int? SuplenteId { get; set; }

    }
    public record DepartamentoCreateDTO(string Nombre, decimal PresupuestoAnual, int? ResponsableId, int? SuplenteId);

    public record DepartamentoResponseDTO(int Id,string Nombre,decimal PresupuestoAnual,string? ResponsableNombre,string? SuplenteNombre);
}
