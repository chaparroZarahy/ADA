using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using CarritoComprasADA_API.Models;
using System.Reflection;
using CarritoComprasADA_API.Helpers;

namespace CarritoComprasADA_API.Controllers
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public ProductController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        [HttpGet("")]
        public IActionResult ObtenerProductos()
        {

            try
            {
                var productos = new List<Producto>();
                using var command = new SqlCommand("obtener_producto", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                _connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    productos.Add(new Producto
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Cantidad = reader.GetInt32(2),
                        Descripcion = reader.IsDBNull(3) ? "" : reader.GetString(3)
                    });

                }

                return Ok(productos);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }
    }
}