using System.ComponentModel.DataAnnotations;

namespace CarritoComprasADA.Models
{
    public class Login
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasena { get; set; }
    }
}
