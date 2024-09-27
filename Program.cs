
var builder = WebApplication.CreateBuilder(args);


// Agregar servicios al contenedor.
builder.Services.AddControllers(); // Registra todos los controladores, incluido el DynamicController
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar el pipeline de solicitudes HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Endpoint existente: WeatherForecast
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Ejemplo de un endpoint mapeado manualmente
app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Mapear controladores dinámicamente, incluye el nuevo controlador DynamicController.
app.MapControllers(); // Esto escanea y configura automáticamente todos los controladores en el proyecto

// Puedes agregar rutas adicionales dinámicas aquí si necesitas rutas específicas no manejadas por controladores
// Ejemplo para manejar rutas personalizadas sin crear nuevos controladores:
// app.MapGet("/{dynamicRoute}", async context => 
// {
//     var route = context.Request.RouteValues["dynamicRoute"]?.ToString();
//     await context.Response.WriteAsync($"Ruta dinámica recibida: {route}");
// });

app.Run();

// Definición del record WeatherForecast
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


