using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToPage("/Login/Login"); 
        }
    }
}