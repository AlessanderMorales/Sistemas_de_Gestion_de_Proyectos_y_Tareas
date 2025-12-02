using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using System.Security.Claims;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Proyectos
{
    [Authorize]
    public class CalendarioModel : PageModel
    {
     private readonly ProyectoApiClient _proyectoApi;

   public List<ProyectoDTO> Proyectos { get; set; } = new();

        public CalendarioModel(ProyectoApiClient proyectoApi)
        {
            _proyectoApi = proyectoApi;
        }

        public async Task OnGetAsync()
        {
  if (User.IsInRole("Empleado"))
    {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

     if (int.TryParse(idClaim, out var usuarioId))
    {
          Proyectos = await _proyectoApi.GetByUsuarioAsync(usuarioId);
      }
         }
else
        {
      Proyectos = await _proyectoApi.GetAllAsync();
     }

          // Ordenar proyectos por fecha de inicio (los más próximos primero)
            Proyectos = Proyectos
        .Where(p => p.FechaInicio.HasValue)
      .OrderBy(p => p.FechaInicio)
             .ToList();
        }
    }
}
