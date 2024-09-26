using Microsoft.AspNetCore.Mvc;

namespace SimpleApi.Controllers
{
    [ApiController]
    [Route("[controller]/{dynamicRoute?}")] // Ruta base con un parámetro de ruta opcional
    public class DynamicController : ControllerBase
    {
        // Endpoint GET para manejar solicitudes GET con rutas dinámicas
        [HttpGet]
        public IActionResult Get([FromRoute] string dynamicRoute)
        {
            if (string.IsNullOrEmpty(dynamicRoute))
            {
                return BadRequest("Ruta no especificada.");
            }

            return Ok(new { Message = $"Ruta dinámica recibida: {dynamicRoute}" });
        }

        // Endpoint GET para obtener datos específicos por ID
        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] string dynamicRoute, [FromRoute] int id)
        {
            if (string.IsNullOrEmpty(dynamicRoute))
            {
                return BadRequest("Ruta no especificada.");
            }

            // Simulación de búsqueda de datos por ID
            var data = new { Id = id, Name = "Producto" + id, Description = "Descripción del producto " + id };

            return Ok(new { Message = $"Datos recibidos para ID: {id} en la ruta: {dynamicRoute}", Data = data });
        }

        // Endpoint POST para insertar datos
        [HttpPost("insert")]
        public IActionResult Insert([FromRoute] string dynamicRoute, [FromBody] object data)
        {
            if (string.IsNullOrEmpty(dynamicRoute))
            {
                return BadRequest("Ruta no especificada.");
            }

            // Lógica para insertar los datos
            return Ok(new { Message = $"Datos insertados en la ruta: {dynamicRoute}", Data = data });
        }

        // Endpoint PUT para actualizar datos usando ID
        // Ruta: /Dynamic/{dynamicRoute}/update/{id}
        [HttpPut("update/{id}")]
        public IActionResult Update([FromRoute] string dynamicRoute, [FromRoute] int id, [FromBody] object data)
        {
            if (string.IsNullOrEmpty(dynamicRoute))
            {
                return BadRequest("Ruta no especificada.");
            }

            // Lógica para actualizar los datos usando el ID
            return Ok(new { Message = $"Datos actualizados para ID: {id} en la ruta: {dynamicRoute}", Data = data });
        }

        // Endpoint DELETE para eliminar datos por ID
        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] string dynamicRoute, [FromRoute] int id)
        {
            if (string.IsNullOrEmpty(dynamicRoute))
            {
                return BadRequest("Ruta no especificada.");
            }

            // Simulación de eliminación de datos por ID
            return Ok(new { Message = $"Datos eliminados para ID: {id} en la ruta: {dynamicRoute}" });
        }
    }
}
