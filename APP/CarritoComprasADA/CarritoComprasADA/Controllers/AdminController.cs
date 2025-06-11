using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using CarritoComprasADA.Models;
using Newtonsoft.Json;

namespace CarritoComprasADA_API.Controllers
{
    public class AdminController : Controller
    {
        string mensaje = "";

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly SqlConnection _connection;
        private readonly HttpClient _httpClient;
        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _connection = new SqlConnection(_connectionString);

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");

        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CrearProducto()
        {
            return View();
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> Producto()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/producto");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var productos = JsonConvert.DeserializeObject<List<Product>>(jsonResponse);

                    return View(productos);
                }
                else
                {
                    ViewBag.Error = "No se pudieron obtener los productos del servidor.";
                    return View(new List<Product>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al conectar con el servidor: " + ex.Message;
                return View(new List<Product>());
            }
        }
        [HttpPost]
        public IActionResult ActualizarProducto(Product producto)
        {
            using var command = new SqlCommand("actualizar_producto", _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@productoId", producto.Id);
            command.Parameters.AddWithValue("@cantidad", producto.Cantidad);


            if (HttpContext.Session.GetString("usuarioId") != null)
            {
                command.Parameters.AddWithValue("@usuarioId", Convert.ToInt32(HttpContext.Session.GetString("usuarioId")));
            }
            else
            {
                var usuarioIdEnSesion = HttpContext.Session.GetString("usuarioId");

                if (string.IsNullOrEmpty(usuarioIdEnSesion))
                {
                    return Unauthorized();
                }

                command.Parameters.AddWithValue("@usuarioId", Convert.ToInt32(usuarioIdEnSesion));

            }

            _connection.Open();
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                mensaje = reader.GetString(0); 
            }

            _connection.Close();

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Producto");
        }


        [HttpGet]
        public IActionResult Usuario()
        {
            var clientes = new List<UserClient>();

            using var command = new SqlCommand("obtener_usuario", _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            _connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(new UserClient
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Usuario = reader.GetString(2),
                    Identificacion = reader.GetString(3),
                    Telefono = reader.GetString(4)
                });
            }

            return View(clientes);
        }

        [HttpGet]
        public IActionResult Venta()
        {
            var venta = new List<Venta>();
            using var command = new SqlCommand("obtener_venta", _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            _connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                 venta.Add(new Venta
                {
                    Id = reader.GetInt32(0),
                    Cliente = reader.GetString(1),
                    Producto = reader.GetString(2),
                    Cantidad = reader.GetInt32(3),
                    Fecha = reader.GetDateTime(4)
                });
            }

            return View(venta);
        }

        [HttpPost]
        public IActionResult CrearProducto(Product producto)
        {
          
            using var command = new SqlCommand("registrar_producto", _connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Nombre", producto.Nombre);
            command.Parameters.AddWithValue("@Cantidad", producto.Cantidad);
            command.Parameters.AddWithValue("@Descripcion", producto.Descripcion);

            _connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
                mensaje = reader.GetString(0);

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Producto");
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear(); 
            return RedirectToAction("Index", "Login");
        }

    }
}
