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
    [Route("api/user")]
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

        [HttpPost("register")]
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
                command.ExecuteNonQuery();
                _connection.Close();

                return Ok(new { mensaje = "Usuario registrado exitosamente." });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor.", error = ex.Message });
            }
        }
    }
}