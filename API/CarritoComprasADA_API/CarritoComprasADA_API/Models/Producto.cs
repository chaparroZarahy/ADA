﻿namespace CarritoComprasADA_API.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public int Cantidad { get; set; }
        public string? Descripcion { get; set; }
        public int? UsuarioId { get; set; }

    }
}
