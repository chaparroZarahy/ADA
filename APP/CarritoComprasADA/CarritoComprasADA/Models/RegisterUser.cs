using System.ComponentModel.DataAnnotations;

namespace CarritoComprasADA.Models
{
    public class RegisterUser
    {

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono solo debe contener números.")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "La contraseña debe tener letras y números.")]
        public string Contrasena { get; set; }
    }

}

