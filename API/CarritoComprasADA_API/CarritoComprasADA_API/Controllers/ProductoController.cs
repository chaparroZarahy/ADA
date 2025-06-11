using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using CarritoComprasADA_API.Models;
using System.Reflection;
using CarritoComprasADA_API.Helpers;
using System.Reflection.PortableExecutable;

namespace CarritoComprasADA_API.Controllers
{
    [ApiController]
    [Route("api/producto")]
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

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] Producto producto)
        {
            if (producto == null)
            {
                return BadRequest(new { mensaje = "El producto no puede ser nulo." });
            }

            try
            {
                using var command = new SqlCommand("registrar_producto", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Nombre", producto.Nombre);
                command.Parameters.AddWithValue("@Cantidad", producto.Cantidad);
                command.Parameters.AddWithValue("@Descripcion", string.IsNullOrEmpty(producto.Descripcion) ? (object)DBNull.Value : producto.Descripcion);

                _connection.Open();
                using var reader = command.ExecuteReader();

                int codigo = 0;
                string mensaje = "Producto creado correctamente.";

                if (reader.Read())
                {
                    codigo = reader.GetInt32(0);
                    mensaje = reader.GetString(1);
                }

                return Ok(new { codigo, mensaje });

            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { mensaje = "Error de base de datos", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error inesperado", error = ex.Message });
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }

        [HttpPut("actualizar")]
        public IActionResult Actualizar([FromBody] Producto producto, [FromQuery] int id)
        {
            if (producto == null)
            {
                return BadRequest(new { mensaje = "El producto no puede ser nulo." });
            }

            try
            {
                using var command = new SqlCommand("actualizar_producto", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@productoId", id);
                command.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                command.Parameters.AddWithValue("@usuarioId", producto.UsuarioId);

                _connection.Open();
                using var reader = command.ExecuteReader();

                string mensaje = "Producto actualizado correctamente.";
                if (reader.Read())
                {
                    mensaje = reader.GetString(0);
                }

                return Ok(new { mensaje });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { mensaje = "Error de base de datos", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error inesperado", error = ex.Message });
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }


    }
}