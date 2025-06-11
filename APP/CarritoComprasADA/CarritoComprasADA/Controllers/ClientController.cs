using CarritoComprasADA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using CarritoComprasADA.Filters;
using Newtonsoft.Json;
using System.Net.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;
using System.Reflection;

namespace CarritoComprasADA_API.Controllers
{
    [AutorizacionTipoUsuario("Cliente")]

    public class ClientController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ClientController(IConfiguration configuration)
        {
            _configuration = configuration;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
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

        public async Task<IActionResult> Comprar(int productoId, int cantidad)
        {
            var usuarioId = HttpContext.Session.GetString("usuarioId");
            if (string.IsNullOrEmpty(usuarioId))
                return RedirectToAction("Login");

            var venta = new RegistrarVenta
            {
                UsuarioId = int.Parse(usuarioId),
                ProductoId = productoId,
                Cantidad = cantidad
            };

            var json = JsonConvert.SerializeObject(venta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("api/venta/registrar", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(result);

                TempData["Mensaje"] = responseData?.mensaje.ToString();

                if (int.TryParse(responseData?.cantidadDisponible?.ToString(), out int cantidadValue))
                {
                    TempData["ProductoId"] = productoId;
                    TempData["CantidadDisponible"] = cantidadValue;
                }

            }
            else
            {
                TempData["Mensaje"] = "Error al procesar la compra.";
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> confirmarCompra(int ProductoId, int Cantidad)
        {
            string mensaje;

            var usuarioId = HttpContext.Session.GetString("usuarioId");
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();


            var venta = new RegistrarVenta
            {
                UsuarioId = int.Parse(usuarioId),
                ProductoId = ProductoId,
                Cantidad = Cantidad
            };

            var json = JsonConvert.SerializeObject(venta);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync("api/venta/confirmar", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(result);

                mensaje = responseData.mensaje.ToString();

            }
            else
            {
                mensaje = "Error al registrar la compra";
            }

            TempData["Mensaje"] = mensaje;
            return RedirectToAction("Index");

        }


        public async Task<IActionResult> Historial()
        {
            var usuarioId = HttpContext.Session.GetString("usuarioId");
            if (string.IsNullOrEmpty(usuarioId))
                return Unauthorized();

            try
            {
                string url = $"api/venta/historial?usuarioId={Uri.EscapeDataString(usuarioId)}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
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

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }


    }
}

