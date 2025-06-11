using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using CarritoComprasADA_API.Helpers;
using CarritoComprasADA.Models;


namespace CarritoComprasADA.Controllers
{
    public class RegisterUserController : Controller
    {
        string mensaje;

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;
        private readonly AesBase64Service _aesService;

        public RegisterUserController(IConfiguration configuration, AesBase64Service aesService)
        {
            _configuration = configuration;
            _aesService = aesService;

            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterUser nuevoUsuario)
        {
            try
            {
                using var command = new SqlCommand("registrar_usuario", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                string encryptedPassword = _aesService.EncryptToBase64(nuevoUsuario.Contrasena);
                command.Parameters.AddWithValue("@Nombre", nuevoUsuario.Nombre);
                command.Parameters.AddWithValue("@Direccion", nuevoUsuario.Direccion);
                command.Parameters.AddWithValue("@Telefono", nuevoUsuario.Telefono);
                command.Parameters.AddWithValue("@Usuario", nuevoUsuario.Usuario);
                command.Parameters.AddWithValue("@Identificacion", nuevoUsuario.Identificacion);
                command.Parameters.AddWithValue("@Contrasena", encryptedPassword);
                command.Parameters.AddWithValue("@RolNombre", "Cliente"); 

                _connection.Open();
                var result = command.ExecuteReader();
                mensaje = result.Read() ? result.GetString(0) : "No se pudo registrar.";
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Register");
        }

    }
}
