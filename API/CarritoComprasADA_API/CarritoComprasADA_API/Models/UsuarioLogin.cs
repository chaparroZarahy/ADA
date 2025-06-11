using System.ComponentModel.DataAnnotations;

namespace CarritoComprasADA_API.Models
{
    public class UsuarioLogin
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
    }
}
