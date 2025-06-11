using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CarritoComprasADA.Filters
{
    public class AutorizacionTipoUsuarioAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _tipoRequerido;

        public AutorizacionTipoUsuarioAttribute(string tipoRequerido)
        {
            _tipoRequerido = tipoRequerido;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var tipoUsuario = context.HttpContext.Session.GetString("Rol");

            if (string.IsNullOrEmpty(tipoUsuario) || tipoUsuario != _tipoRequerido)
            {
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}
