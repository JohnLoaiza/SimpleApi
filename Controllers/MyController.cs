using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace SimpleApi.Controllers
{
    [ApiController]
    [Route("[controller]/{project?}/{collection?}")] // Ruta base con un parámetro de ruta opcional
    public class DynamicController : ControllerBase
    {
        private   string connectionString =  "mongodb://localhost:27017";
        private  MongoClient client;
        private  IMongoDatabase database;

        private  IMongoCollection<Model> collectionL;

        private IMongoCollection<Model>? GetCollection(string project, string collection) {
            client = new MongoClient(connectionString);
            database = client.GetDatabase(project);
          return database.GetCollection<Model>(collection);
        }

        
        // Endpoint GET para manejar solicitudes GET con rutas dinámicas
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromRoute] string project, [FromRoute] string collection)
        {
            if (string.IsNullOrEmpty(project))
            {
                return BadRequest("Ruta no especificada.");
            }
 return Ok(await GetCollection(project, collection).Find(new BsonDocument()).ToListAsync());
         //   return Ok(new { Message = $"Ruta dinámica recibida: {project} y coleccion {collection}" });
        }

        // Endpoint GET para obtener datos específicos por ID
        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] string project, [FromRoute] int id)
        {
            if (string.IsNullOrEmpty(project))
            {
                return BadRequest("Ruta no especificada.");
            }

            // Simulación de búsqueda de datos por ID
            var data = new { Id = id, Name = "Producto" + id, Description = "Descripción del producto " + id };

            return Ok(new { Message = $"Datos recibidos para ID: {id} en la ruta: {project}", Data = data });
        }

        // Endpoint POST para insertar datos
        [HttpPost("insert")]
        public  async Task<IActionResult> InsertAsync([FromRoute] string project, [FromRoute] string collection, [FromBody] JsonElement body)
        {

           

            if (string.IsNullOrEmpty(project))
            {
                return BadRequest("Ruta no especificada.");
            }
            collectionL = GetCollection(project, collection)!;
            Dictionary<string, object?> documento = new Dictionary<string, object?>();

             foreach (JsonProperty propiedad in body.EnumerateObject())
            {
                Console.WriteLine($"Clave: {propiedad.Name}, Valor: {propiedad.Value}");
               documento.Add($"{propiedad.Name}", $"{propiedad.Value}");
            }


            Model newDoc = new Model
        {
            Id = ObjectId.GenerateNewId(), // Generar un nuevo ObjectId para el usuario
            propierties = documento
        };
               

            await collectionL.InsertOneAsync(newDoc);
            // Lógica para insertar los datos
            return Ok(new { Message = $"Datos insertados en la ruta: {project} y coleccion {collection}"});
        }

        // Endpoint PUT para actualizar datos usando ID
        // Ruta: /Dynamic/{dynamicRoute}/update/{id}
        [HttpPut("update/{id}")]
        public IActionResult Update([FromRoute] string project, [FromRoute] int id, [FromBody] object data)
        {
            if (string.IsNullOrEmpty(project))
            {
                return BadRequest("Ruta no especificada.");
            }

            // Lógica para actualizar los datos usando el ID
            return Ok(new { Message = $"Datos actualizados para ID: {id} en la ruta: {project}", Data = data });
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
