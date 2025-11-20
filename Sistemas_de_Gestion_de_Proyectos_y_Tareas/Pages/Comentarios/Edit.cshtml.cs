using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Comentarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Comentarios
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ComentarioApiClient _comentarioApi;

        [BindProperty]
        public ComentarioDTO Comentario { get; set; } = new();

        public EditModel(ComentarioApiClient comentarioApi)
        {
            _comentarioApi = comentarioApi;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var comentario = await _comentarioApi.GetByIdAsync(id);

            if (comentario == null)
                return NotFound();

            Comentario = comentario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            int id = Comentario.IdComentario;

            var original = await _comentarioApi.GetByIdAsync(id);

            Comentario.IdUsuario = original.IdUsuario;
            Comentario.IdTarea = original.IdTarea;
            Comentario.IdDestinatario = original.IdDestinatario;
            Comentario.Estado = original.Estado;
            Comentario.Fecha = original.Fecha;

            var result = await _comentarioApi.UpdateAsync(id, Comentario);

            if (!result)
            {
                TempData["ErrorMessage"] = "No se pudo actualizar el comentario.";
                return Page();
            }

            TempData["SuccessMessage"] = "Comentario actualizado exitosamente.";
            return RedirectToPage("Index");
        }
    }
}
