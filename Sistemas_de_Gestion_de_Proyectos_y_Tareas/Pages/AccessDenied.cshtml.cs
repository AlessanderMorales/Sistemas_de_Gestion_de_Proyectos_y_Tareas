using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages
{
    [AllowAnonymous]
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}