using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
namespace SimpleApi.Controllers
{
    [ApiController]
    [Route("[controller]/{project?}/{collection?}")] // Ruta base con un parámetro de ruta opcional
    public class DynamicController : ControllerBase
    {
        private string connectionString = "mongodb+srv://jhon91811:CWFFyqphmgKv5nFl@cluster0.hkyi7.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";  //"mongodb://localhost:27017";
        private MongoClient? client;
        private IMongoDatabase? database;

        private IMongoCollection<Model>? collectionL;

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
                JsonValueKind.Object => element.EnumerateObject()
                                               .ToDictionary(prop => prop.Name, prop => ConvertJsonElementToType(prop.Value)),
                JsonValueKind.Array => element.EnumerateArray()
                                              .Select(ConvertJsonElementToType)
                                              .ToList(),
                _ => throw new NotSupportedException($"Tipo de JSON no soportado: {element.ValueKind}")
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
                properties = documento
            };


            await collectionL.InsertOneAsync(newDoc);
            // Lógica para insertar los datos
            return Ok(new { Message = $"Datos insertados en la ruta: {project} y coleccion {collection}", id = newDoc.Id });
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
                // Agregar cada propiedad como una actualización dentro del diccionario 'properties', con conversión de tipo
                updates.Add(Builders<Model>.Update.Set($"properties.{property.Name}", ConvertJsonElementToType(property.Value)));
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


        // Método POST para encriptar una contraseña
        [HttpPost("encrypt")]
        public IActionResult Encrypt([FromBody] JsonElement body)
        {
            Console.WriteLine("encripta");
            if (body.TryGetProperty("data", out JsonElement data))
            {
                string? dataValue = data.GetString();
                if (string.IsNullOrEmpty(dataValue))
                {
                    return BadRequest("La contraseña no puede estar vacía.");
                }
                // Encriptar la contraseña
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dataValue);

                return Ok(new { HashedPassword = hashedPassword });
            }
            else
            {
                return BadRequest("La contraseña no puede estar vacía.");
            }

        }

        // Método POST para verificar una contraseña
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEncryptAsync(
    [FromRoute] string project,
    [FromRoute] string collection,
    [FromBody] JsonElement body)
        {
            if (string.IsNullOrEmpty(project) || string.IsNullOrEmpty(collection))
            {
                return BadRequest("Proyecto o colección no especificados.");
            }

            // Extraer parámetros del cuerpo de la solicitud
            string parameterName = body.GetProperty("userField").GetString() ?? string.Empty;
            string passwordField = body.GetProperty("encryptedField").GetString() ?? string.Empty;
            string password = body.GetProperty("encrypted").GetString() ?? string.Empty;
            string valueToCompare = body.GetProperty("user").GetString() ?? string.Empty;

            if (string.IsNullOrEmpty(parameterName) || string.IsNullOrEmpty(passwordField) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(valueToCompare))
            {
                return BadRequest("Todos los parámetros son requeridos.");
            }

            var collectionL = GetCollection(project, collection)!;

            // Filtro para buscar el documento
            var filter = Builders<Model>.Filter.Eq($"properties.{parameterName}", valueToCompare);
            var document = await collectionL.Find(filter).FirstOrDefaultAsync();

            if (document == null)
            {
                return NotFound(new { success = false, Message = $"No se encontró el documento con {parameterName}: {valueToCompare} en la colección {collection} del proyecto {project}." });
            }

            // Obtener la contraseña encriptada almacenada en el documento
            if (document.properties.TryGetValue(passwordField, out var hashedPassword) && hashedPassword is string hashedPasswordString)
            {
                // Verificar la contraseña
                bool isPasswordMatch = BCrypt.Net.BCrypt.Verify(password, hashedPasswordString);

                if (isPasswordMatch)
                {
                    // Generar JWT
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_256_bit_secret_key_here_which_is_at_least_32_characters_long"));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var claims = new[]
                    {
                new Claim(JwtRegisteredClaimNames.Sub, document.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                    var token = new JwtSecurityToken(
                        issuer: "your_issuer_here", // Define tu emisor
                        audience: "your_audience_here", // Define tu audiencia
                        claims: claims,
                        expires: DateTime.UtcNow.AddHours(1), // Tiempo de expiración
                        signingCredentials: creds
                    );

                    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                    return Ok(new { success = true, id = document.Id, Message = "Las contraseñas coinciden.", Token = jwt });
                }
                else
                {
                    return Unauthorized(new { success = false, Message = "Las contraseñas no coinciden." });
                }
            }
            else
            {
                return BadRequest(new { success = false, Message = $"El campo de contraseña {passwordField} no existe en el documento." });
            }
        }


        // Método para verificar si el JWT está vigente
        [HttpPost("verify-token")]
        public IActionResult VerifyToken([FromBody] JsonElement body)
        {
            try
            {
                string token = body.GetProperty("token").GetString() ?? string.Empty;

                var result = IsTokenValid(token);

                if (result.IsValid)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "El token es válido.",
                        timeRemaining = result.TimeRemaining
                    });
                }
                else
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "El token ha expirado o es inválido."
                    });
                }
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "Hubo un error al procesar el token." });
            }
        }

        // Método auxiliar para verificar si el JWT está vigente
        private (bool IsValid, string TimeRemaining) IsTokenValid(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return (false, string.Empty);
                }

                // Obtener el claim de expiración (exp) del token
                var expiration = jsonToken?.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (string.IsNullOrEmpty(expiration))
                {
                    return (false, string.Empty);
                }

                // Convertir la fecha de expiración a DateTime
                var expirationDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiration)).UtcDateTime;

                // Calcular el tiempo restante
                var timeRemaining = expirationDate - DateTime.UtcNow;

                if (timeRemaining > TimeSpan.Zero)
                {
                    return (true, timeRemaining.ToString(@"hh\:mm\:ss"));
                }
                else
                {
                    return (false, string.Empty);
                }
            }
            catch (Exception)
            {
                // Si el token no es válido o si ocurre algún error, lo consideramos como inválido
                return (false, string.Empty);
            }
        }
    }
}
