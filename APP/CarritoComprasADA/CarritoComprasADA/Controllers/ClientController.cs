using CarritoComprasADA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;

namespace CarritoComprasADA_API.Controllers
{
    public class ClientController : Controller
    {
        string mensaje = "";

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;

        public ClientController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);

        }

        [HttpGet]
        public IActionResult Index()
        {
            var productos = new List<Product>();
            using var command = new SqlCommand("obtener_producto", _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            _connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                productos.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Cantidad = reader.GetInt32(2),
                    Descripcion = reader.IsDBNull(3) ? "" : reader.GetString(3)
                });
            }

            return View(productos);
        }

        [HttpPost]
        public IActionResult Comprar(int ProductoId, int Cantidad)
        {

            var usuarioId = HttpContext.Session.GetString("usuarioId");

            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();

            try
            {
                using var command = new SqlCommand("realizar_compra", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UsuarioId", Convert.ToInt32(usuarioId));
                command.Parameters.AddWithValue("@ProductoId", ProductoId);
                command.Parameters.AddWithValue("@Cantidad", Cantidad);

                _connection.Open();

                var result = command.ExecuteScalar();
                mensaje = result?.ToString() ?? mensaje;
                if (mensaje.Contains("¿Desea comprarlas?"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(mensaje, @"Solo hay (\d+)");
                    if (match.Success)
                    {
                        TempData["ProductoId"] = ProductoId;
                        TempData["CantidadDisponible"] = match.Groups[1].Value;
                    }
                }

            }
            catch (Exception ex)
            {
                mensaje = "Error en la operación: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult confirmarcompra(int ProductoId, int Cantidad)
        {
            var usuarioId = HttpContext.Session.GetString("usuarioId");
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();


            try
            {
                using var command = new SqlCommand("confirmar_compra", _connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@UsuarioId", Convert.ToInt32(usuarioId));
                command.Parameters.AddWithValue("@ProductoId", ProductoId);
                command.Parameters.AddWithValue("@Cantidad", Cantidad);

                _connection.Open();
                var result = command.ExecuteScalar();
                if (result != null)
                    mensaje = result.ToString();
            }
            catch (Exception ex)
            {
                mensaje = "Error en la operación: " + ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Index");
        }


        public IActionResult Historial()
        {
            var usuarioId = HttpContext.Session.GetString("usuarioId");
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();

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
            _connection.Close();

            return View(ventas);
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index", "Login");
        }


    }
}

