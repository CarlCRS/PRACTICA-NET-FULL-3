namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class ConfigRevisor
    {
        public int Id { get; set; }
        public string Rol { get; set; } = string.Empty;   
        public int ColaboradorId { get; set; }
        public int? SuplenteId { get; set; }
    }

    public record ConfigRevisorDTO(int Id, string Rol, int ColaboradorId, string? ColaboradorNombre,
        int? SuplenteId, string? SuplenteNombre);
}
