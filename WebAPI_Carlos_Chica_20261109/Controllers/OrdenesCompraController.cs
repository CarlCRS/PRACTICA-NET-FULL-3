using Microsoft.AspNetCore.Mvc;
using WebAPI_Carlos_Chica_20261109.Model;
using WebAPI_Carlos_Chica_20261109.Service;

namespace WebAPI_Carlos_Chica_20261109.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesCompraController : ControllerBase
    {
        private readonly OrdenCompraService _service;

        public OrdenesCompraController(OrdenCompraService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Listar() => Ok(await _service.ListarOrdenesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var orden = await _service.ObtenerOrdenAsync(id);
            if (orden == null) return NotFound();
            return Ok(orden);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] OrdenCompraCrearRequest request)
        {
            if (request == null || request.Detalles == null || request.Detalles.Count == 0)
                return BadRequest("Detalles requeridos.");

            var detalles = request.Detalles.Select(d =>
                new OrdenCompraDetalleCreateDTO(d.Descripcion, d.Cantidad, d.PrecioUnitario)).ToList();

            int? id = await _service.CrearOrdenAsync(request.SolicitudId, request.ProveedorId, detalles);
            if (id == null)
                return BadRequest("No se pudo crear la orden.");

            return CreatedAtAction(nameof(Obtener), new { id }, id);
        }
    }

    public record OrdenCompraCrearRequest(int SolicitudId, int ProveedorId,
        List<OrdenCompraDetalleCrearRequest2> Detalles);

    public record OrdenCompraDetalleCrearRequest2(string Descripcion, decimal Cantidad, decimal PrecioUnitario);
}
