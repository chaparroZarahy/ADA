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
    [Route("api/venta")]
    public class VentaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public VentaController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        [HttpGet("")]
        public IActionResult Obtener()
        {

            try
            {
                var ventas = new List<Venta>();
                using var command = new SqlCommand("obtener_venta", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                _connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ventas.Add(new Venta
                    {
                        Id = reader.GetInt32(0),
                        Cliente = reader.GetString(1),
                        Producto = reader.GetString(2),
                        Cantidad = reader.GetInt32(3),
                        Fecha = reader.GetDateTime(4)
                    });
                }

                return Ok(ventas);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }


        [HttpGet("historial")]
        public IActionResult ObtenerHistorial(string usuarioId)
        {

            try
            {
                var ventas = new List<Venta>();
                using var command = new SqlCommand("obtener_historial_compras", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@UsuarioId", Convert.ToInt32(usuarioId));

                _connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    ventas.Add(new Venta
                    {
                        Id = reader.GetInt32(0),
                        Producto = reader.GetString(1),
                        Cantidad = reader.GetInt32(2),
                        Fecha = reader.GetDateTime(3)
                    });
                }

                return Ok(ventas);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }

        [HttpPost("registrar")]
        public IActionResult Registrar([FromBody] RegistrarVenta venta)
        {
            string mensaje = "Producto comprado correctamente.";
            int? cantidadDisponible = null;

            try
            {
                using var command = new SqlCommand("realizar_compra", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UsuarioId", venta.UsuarioId);
                command.Parameters.AddWithValue("@ProductoId", venta.ProductoId);
                command.Parameters.AddWithValue("@Cantidad", venta.Cantidad);

                _connection.Open();
                var result = command.ExecuteScalar();
                mensaje = result?.ToString() ?? mensaje;

                if (mensaje.Contains("¿Desea comprarlas?"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(mensaje, @"Solo hay (\d+)");
                    if (match.Success)
                        cantidadDisponible = int.Parse(match.Groups[1].Value);
                }

                return Ok(new
                {
                    mensaje,
                    productoId = venta.ProductoId,
                    cantidadSolicitada = venta.Cantidad,
                    cantidadDisponible
                });
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
                    _connection.Close();
            }
        }

        [HttpPost("confirmar")]
        public IActionResult Confirmar([FromBody] RegistrarVenta venta)
        {
            string mensaje = "Producto comprado correctamente.";

            try
            {
                using var command = new SqlCommand("confirmar_compra", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UsuarioId", venta.UsuarioId);
                command.Parameters.AddWithValue("@ProductoId", venta.ProductoId);
                command.Parameters.AddWithValue("@Cantidad", venta.Cantidad);

                _connection.Open();
                var result = command.ExecuteScalar();
                mensaje = result?.ToString() ?? mensaje;


                return Ok(new
                {
                    mensaje
                });
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
                    _connection.Close();
            }
        }

    }
}