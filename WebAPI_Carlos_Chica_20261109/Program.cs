using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI_Carlos_Chica_20261109.Repository;
using WebAPI_Carlos_Chica_20261109.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var conexion = builder.Configuration.GetConnectionString("Base_Conexion")
    ?? throw new Exception("No se encontro la cadena de conexion 'Base_Conexion' en appsettings.json.");

builder.Services.AddScoped<IDbConnection>(sp => new SqlConnection(conexion));

builder.Services.AddScoped<CatalogoRepository>();
builder.Services.AddScoped<SolicitudService>();
builder.Services.AddScoped<OrdenCompraService>();
builder.Services.AddScoped<RecepcionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirAngular");

app.UseAuthorization();

app.MapControllers();

app.Run();
