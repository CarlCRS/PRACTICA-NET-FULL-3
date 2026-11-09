namespace WebAPI_Carlos_Chica_20261109.Model
{
    public class Colaborador
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }
    public record ColaboradorDTO(string Cedula, string Nombre, string Correo);
    public record ColaboradorResponseDTO(int Id, string Cedula, string Nombre, string Correo);
}
