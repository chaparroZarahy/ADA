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
    [Route("api/usuario")]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AesBase64Service _aesService;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public UsuarioController(IConfiguration configuration, AesBase64Service aesService)
        {
            _configuration = configuration;
            _aesService = aesService;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        [HttpPost("registrar")]
        public IActionResult Register([FromBody] RegistrarUsuario request)
        {
            try
            {
                string encryptedPassword = _aesService.EncryptToBase64(request.Contrasena);

                using var command = new SqlCommand("registrar_usuario", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@Nombre", request.Nombre);
                command.Parameters.AddWithValue("@Direccion", request.Direccion);
                command.Parameters.AddWithValue("@Telefono", request.Telefono);
                command.Parameters.AddWithValue("@Usuario", request.Usuario);
                command.Parameters.AddWithValue("@Identificacion", request.Identificacion);
                command.Parameters.AddWithValue("@Contrasena", encryptedPassword);
                command.Parameters.AddWithValue("@RolNombre", "Cliente");

                _connection.Open();

                string mensaje = string.Empty;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        mensaje = reader.GetString(0); // capturamos el SELECT 'mensaje'
                    }
                }

                _connection.Close();

                // Revisamos si el mensaje es un error esperado
                if (mensaje.Contains("ya está en uso") || mensaje.Contains("ya está registrada") || mensaje.Contains("obligatorios") || mensaje.Contains("ya está registrado"))
                {
                    return BadRequest(new { mensaje }); // 400 para errores de validación
                }

                return Ok(new { mensaje }); // éxito

            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { mensaje = "Error en base de datos.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }



        [HttpGet("")]
        public IActionResult Obtener()
        {

            try
            {
                var clientes = new List<Cliente>();
                using var command = new SqlCommand("obtener_usuario", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                _connection.Open();
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    clientes.Add(new Cliente
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Usuario = reader.GetString(2),
                        Identificacion = reader.GetString(3),
                        Telefono = reader.GetString(4)
                    });
                }

                return Ok(clientes);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }
    }
}