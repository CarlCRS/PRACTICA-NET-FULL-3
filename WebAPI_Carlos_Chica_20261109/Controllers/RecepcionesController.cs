using Microsoft.AspNetCore.Mvc;
using WebAPI_Carlos_Chica_20261109.Service;

namespace WebAPI_Carlos_Chica_20261109.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecepcionesController : ControllerBase
    {
        private readonly RecepcionService _service;

        public RecepcionesController(RecepcionService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar(int ordenCompraId) => Ok(await _service.ListarRecepcionesAsync(ordenCompraId));

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] RecepcionCrearRequest request)
        {
            if (request == null) return BadRequest("Datos requeridos.");

            int? id = await _service.CrearRecepcionAsync(request.OrdenCompraId, request.ColaboradorId, request.Observaciones);
            if (id == null) return NotFound("La orden de compra no existe.");

            return CreatedAtAction(nameof(Listar), new { ordenCompraId = request.OrdenCompraId }, id);
        }
    }

    public record RecepcionCrearRequest(int OrdenCompraId, int ColaboradorId, string? Observaciones);
}
