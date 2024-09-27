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
        private string connectionString = "mongodb://localhost:27017";
        private MongoClient client;
        private IMongoDatabase database;

        private IMongoCollection<Model> collectionL;

        private IMongoCollection<Model>? GetCollection(string project, string collection)
        {
            client = new MongoClient(connectionString);
            database = client.GetDatabase(project);
            return database.GetCollection<Model>(collection);
        }

        // Método para convertir JsonElement a un tipo de dato manejable
        private object? ConvertJsonElementToType(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetInt32(out int intValue) => intValue,
                JsonValueKind.Number when element.TryGetDouble(out double doubleValue) => doubleValue,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => element.GetRawText() // Para otros tipos como objetos o arreglos, se puede ajustar según se necesite
            };
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
        public async Task<IActionResult> GetByIdAsync([FromRoute] string project, [FromRoute] string collection, [FromRoute] string id)
        {
            if (string.IsNullOrEmpty(project) || string.IsNullOrEmpty(collection))
            {
                return BadRequest("Proyecto o colección no especificados.");
            }

            var collectionL = GetCollection(project, collection)!;

            // Buscar el documento por id
            var filter = Builders<Model>.Filter.Eq("_id", id);
            var document = await collectionL.Find(filter).FirstOrDefaultAsync();

            if (document == null)
            {
                return NotFound(new { Message = $"No se encontró el documento con ID: {id} en la colección {collection} del proyecto {project}." });
            }

            return Ok(document);
        }

        // Endpoint POST para insertar datos
        [HttpPost("insert")]
        public async Task<IActionResult> InsertAsync([FromRoute] string project, [FromRoute] string collection, [FromBody] JsonElement body)
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
                documento.Add($"{propiedad.Name}", ConvertJsonElementToType(propiedad.Value));
            }


            Model newDoc = new Model
            {
                Id = ObjectId.GenerateNewId().ToString(), // Generar un nuevo ObjectId para el usuario
                propierties = documento
            };


            await collectionL.InsertOneAsync(newDoc);
            // Lógica para insertar los datos
            return Ok(new { Message = $"Datos insertados en la ruta: {project} y coleccion {collection}",id = newDoc.Id });
        }

        // Endpoint PUT para actualizar datos usando ID
        // Ruta: /Dynamic/{dynamicRoute}/update/{id}
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAsync([FromRoute] string project, [FromRoute] string collection, [FromRoute] string id, [FromBody] JsonElement body)
{
    if (string.IsNullOrEmpty(project) || string.IsNullOrEmpty(collection))
    {
        return BadRequest("Proyecto o colección no especificados.");
    }



    var collectionL = GetCollection(project, collection)!;

    // Convertir el cuerpo de la solicitud a un diccionario para aplicar la actualización
    var updates = new List<UpdateDefinition<Model>>();
    foreach (JsonProperty property in body.EnumerateObject())
    {
        // Agregar cada propiedad como una actualización dentro del diccionario 'propierties', con conversión de tipo
        updates.Add(Builders<Model>.Update.Set($"propierties.{property.Name}", ConvertJsonElementToType(property.Value)));
    }

    // Combinar todas las actualizaciones en una sola definición
    var updateDefinition = Builders<Model>.Update.Combine(updates);

    // Aplicar la actualización al documento que coincida con el ObjectId
    var result = await collectionL.UpdateOneAsync(
        Builders<Model>.Filter.Eq("_id", id),
        updateDefinition
    );

    // Verificar si se modificó algún documento
    if (result.ModifiedCount == 0)
    {
        return NotFound(new { Message = $"No se encontró el documento con ID: {id} en la colección {collection} del proyecto {project}." });
    }

    return Ok(new { Message = $"Datos actualizados para ID: {id} en la colección {collection} del proyecto {project}." });
}




       // Endpoint DELETE para eliminar datos por ID
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteAsync([FromRoute] string project, [FromRoute] string collection, [FromRoute] string id)
{
    if (string.IsNullOrEmpty(project) || string.IsNullOrEmpty(collection))
    {
        return BadRequest("Proyecto o colección no especificados.");
    }

   
    // Obtener la colección correspondiente
    var collectionL = GetCollection(project, collection)!;

    // Filtro para buscar el documento por su ID
    var filter = Builders<Model>.Filter.Eq("_id", id);

    // Intentar eliminar el documento
    var result = await collectionL.DeleteOneAsync(filter);

    // Verificar si se eliminó algún documento
    if (result.DeletedCount == 0)
    {
        return NotFound(new { Message = $"No se encontró el documento con ID: {id} en la colección {collection} del proyecto {project}." });
    }

    return Ok(new { Message = $"Datos eliminados para ID: {id} en la colección {collection} del proyecto {project}." });
}

    }
}
