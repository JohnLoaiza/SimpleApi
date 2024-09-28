
var builder = WebApplication.CreateBuilder(args);

// Configurar CORS para permitir cualquier origen, método y encabezado
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


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
     app.UseDeveloperExceptionPage();
}

app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers(); 

app.Run();


