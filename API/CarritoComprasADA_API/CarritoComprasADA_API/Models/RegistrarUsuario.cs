using System.ComponentModel.DataAnnotations;

namespace CarritoComprasADA_API.Models
{
    public class RegistrarUsuario
    {

        public string Nombre { get; set; }
        public string Direccion { get; set; }

        public string Telefono { get; set; }

        public string Usuario { get; set; }

        public string Identificacion { get; set; }

        public string Contrasena { get; set; }
    }

}

