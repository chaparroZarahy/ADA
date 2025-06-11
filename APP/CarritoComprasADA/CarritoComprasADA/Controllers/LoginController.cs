using Microsoft.AspNetCore.Mvc;
using CarritoComprasADA.Models;
using CarritoComprasADA_API.Helpers;
using System.Text;
using Newtonsoft.Json;


namespace CarritoComprasADA_API.Controllers
{
    public class LoginController : Controller
    {
        string mensaje;

        private readonly IConfiguration _configuration;
        private readonly AesBase64Service _aesService;
        private readonly HttpClient _httpClient;

        public LoginController(IConfiguration configuration, AesBase64Service aesService)
        {
            _configuration = configuration;
            _aesService = aesService;

            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7232/");
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View(new Login());
        }

        [HttpPost]
        public async Task<IActionResult> Index(Login model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Por favor complete todos los campos.";
                return View(model);
            }

            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/autenticacion/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<UserLogin>(jsonResponse);

                    HttpContext.Session.SetString("UsuarioNombre", user.Nombre);
                    HttpContext.Session.SetString("Rol", user.Rol);
                    HttpContext.Session.SetString("usuarioId", user.Id.ToString());

                    if (user.Rol == "Administrador")
                        return RedirectToAction("Index", "Admin");
                    else
                        return RedirectToAction("Index", "Client");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos.";
                    return View(model);
                }
                else
                {
                    ViewBag.Error = "Error del servidor. Intenta más tarde.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexión con la base de datos: " + ex.Message;
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/usuario/registrar", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Mensaje"] = "Usuario registrado exitosamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Error al registrar usuario: " + error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error inesperado al registrar el usuario: " + ex.Message);
                return View(model);
            }
        }
    }
}
