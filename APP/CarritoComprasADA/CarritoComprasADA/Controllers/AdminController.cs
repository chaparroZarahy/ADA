using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using CarritoComprasADA.Models;
using Newtonsoft.Json;
using System.Text;
using CarritoComprasADA.Filters;

namespace CarritoComprasADA_API.Controllers
{
    [AutorizacionTipoUsuario("Administrador")]

    public class AdminController : Controller
    {
        string mensaje = "";

        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;

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
        public async Task<IActionResult> ActualizarProducto(Product producto)
        {
            var usuarioIdStr = HttpContext.Session.GetString("usuarioId");
            if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out int usuarioId))
            {
                return Unauthorized();
            }

            producto.UsuarioId = usuarioId;

            try
            {

                string url = $"api/producto/actualizar?id={Uri.EscapeDataString(producto.Id.ToString())}";


                var json = JsonConvert.SerializeObject(producto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync(url, content);


                if (!response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Error al actualizar el producto.";
                    return RedirectToAction("Producto");
                }

                var result = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(result);
                TempData["Mensaje"] = responseData?.mensaje?.ToString() ?? "Producto actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error inesperado: {ex.Message}";
            }

            return RedirectToAction("Producto");
        }


        [HttpGet]
        public async Task<IActionResult> Usuario()
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/usuario");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var productos = JsonConvert.DeserializeObject<List<UserClient>>(jsonResponse);

                    return View(productos);
                }
                else
                {
                    ViewBag.Error = "No se pudieron obtener los usuarios del servidor.";
                    return View(new List<UserClient>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al conectar con el servidor: " + ex.Message;
                return View(new List<UserClient>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Venta()
        {

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/venta");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var ventas = JsonConvert.DeserializeObject<List<Venta>>(jsonResponse);

                    return View(ventas);
                }
                else
                {
                    ViewBag.Error = "No se pudieron obtener las ventas del servidor.";
                    return View(new List<Venta>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al conectar con el servidor: " + ex.Message;
                return View(new List<Venta>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto(Product producto)
        {
            if (!ModelState.IsValid)
            {
                return View(producto);
            }

            try
            {
                var json = JsonConvert.SerializeObject(producto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/producto/registrar", content);

                var result = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(result);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = responseData?.mensaje?.ToString();
                    return RedirectToAction("Producto");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Error al registrar: {error}");
                    return View(producto);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error inesperado: " + ex.Message);
                return View(producto);
            }
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }

    }
}
