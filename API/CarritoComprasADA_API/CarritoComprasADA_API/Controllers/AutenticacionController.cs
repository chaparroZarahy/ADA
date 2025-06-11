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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AesBase64Service _aesService;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public AuthController(IConfiguration configuration, AesBase64Service aesService)
        {
            _configuration = configuration;
            _aesService = aesService;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Login request)
        {

            try
            {
                using var command = new SqlCommand("login_usuario", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                
                var encrypted = _aesService.EncryptToBase64(request.Contrasena);

                command.Parameters.AddWithValue("@Usuario", request.Usuario);
                command.Parameters.AddWithValue("@Contrasena", encrypted);

                _connection.Open();
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var usuario = new UsuarioLogin
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Usuario = reader.GetString(2),
                        Rol = reader.GetString(3)
                    };

                    return Ok(usuario);
                }
                else
                {
                    return Unauthorized(new { mensaje = "Credenciales incorrectas." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }
    }
}