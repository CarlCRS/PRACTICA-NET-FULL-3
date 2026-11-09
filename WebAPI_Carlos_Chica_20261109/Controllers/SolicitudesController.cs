using Microsoft.AspNetCore.Mvc;
using WebAPI_Carlos_Chica_20261109.Model;
using WebAPI_Carlos_Chica_20261109.Service;

namespace WebAPI_Carlos_Chica_20261109.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitudesController : ControllerBase
    {
        private readonly SolicitudService _service;

        public SolicitudesController(SolicitudService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar() => Ok(await _service.ListarSolicitudesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var solicitud = await _service.ObtenerSolicitudAsync(id);
            if (solicitud == null) return NotFound();
            return Ok(solicitud);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] SolicitudCrearRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Justificacion))
                return BadRequest("Justificacion requerida.");
            if (request.Detalles == null || request.Detalles.Count == 0)
                return BadRequest("Debe incluir al menos un detalle.");

            var detalles = request.Detalles.Select(d =>
                new SolicitudDetalleCreateDTO(d.Descripcion, d.Cantidad, d.PrecioUnitario)).ToList();

            int id = await _service.CrearSolicitudAsync(request.SolicitanteId, request.DepartamentoId,
                request.Justificacion, detalles);
            return CreatedAtAction(nameof(Obtener), new { id }, id);
        }

        [HttpPost("{id}/enviar-revision")]
        public async Task<IActionResult> EnviarRevision(int id, [FromBody] EnviarRevisionRequest request)
        {
            if (await _service.EnviarRevisionAsync(id, request.SolicitanteId))
                return Ok(new { mensaje = "Solicitud enviada a revision." });
            return BadRequest("No se pudo enviar. Verifique que este en borrador.");
        }

        [HttpPost("revisiones/{revisionId}/decidir")]
        public async Task<IActionResult> Decidir(int revisionId, [FromBody] DecidirRevisionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Decision))
                return BadRequest("Decision requerida.");

            if (await _service.DecidirRevisionAsync(revisionId, request.RevisorId, request.Decision, request.Comentario))
                return Ok(new { mensaje = $"Revision {request.Decision}." });

            return BadRequest("No se pudo decidir la revision.");
        }

        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            if (await _service.CancelarSolicitudAsync(id))
                return Ok(new { mensaje = "Solicitud cancelada." });
            return BadRequest("No se puede cancelar.");
        }
    }

    public record SolicitudCrearRequest(int SolicitanteId, int DepartamentoId, string Justificacion,
        List<SolicitudDetalleCrearRequest> Detalles);

    public record SolicitudDetalleCrearRequest(string Descripcion, decimal Cantidad, decimal PrecioUnitario);

    public record EnviarRevisionRequest(int SolicitanteId);

    public record DecidirRevisionRequest(int RevisorId, string Decision, string? Comentario);
}
