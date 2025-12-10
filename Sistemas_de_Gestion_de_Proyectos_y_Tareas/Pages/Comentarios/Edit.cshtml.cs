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

            if (comentario == null) return NotFound(); Comentario = comentario;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "❌ Por favor corrija los errores en el formulario antes de continuar.";
                return Page();
            }

            try
            {
                // Validar contenido
                if (string.IsNullOrWhiteSpace(Comentario.Contenido))
                {
                    TempData["ErrorMessage"] = "❌ El contenido del comentario es requerido.";
                    return Page();
                }

                // Sanitizar y validar contenido
                Comentario.Contenido = TrimExtraSpaces(Comentario.Contenido);

                if (ContienePatroneseligrosos(Comentario.Contenido))
                {
                    ModelState.AddModelError("Comentario.Contenido", "⚠️ El comentario contiene caracteres no permitidos.");
                    TempData["ErrorMessage"] = "El comentario contiene caracteres o patrones no permitidos.";
                    return Page();
                }

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
                    TempData["ErrorMessage"] = "❌ No se pudo actualizar el comentario.";
                    return Page();
                }

                TempData["SuccessMessage"] = "✅ Comentario actualizado exitosamente.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.ToLower();

                if (errorMsg.Contains("caracteres") || errorMsg.Contains("patrones") || errorMsg.Contains("sql"))
                {
                    TempData["ErrorMessage"] = $"⚠️ Validación de seguridad: {ex.Message}";
                }
                else
                {
                    TempData["ErrorMessage"] = $"❌ Error al actualizar el comentario: {ex.Message}";
                }

                return Page();
            }
        }

        private string TrimExtraSpaces(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            input = input.Trim();
            return System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");
        }

        private bool ContienePatroneseligrosos(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var patronesPeligrosos = new[]
            {
                @"(\bOR\b|\bAND\b).*=",
                @"['""`;]|--|\/\*|\*\/",
                @"\b(EXEC|EXECUTE|DROP|DELETE|UPDATE|INSERT|SELECT.*FROM|UNION.*SELECT)\b",
                @"<script",
                @"javascript:",
                @"onerror\s*=",
                @"onload\s*=",
                @"<iframe",
                @"[$%^&*(){}[\]\\|]"  // Caracteres especiales peligrosos
            };

            foreach (var patron in patronesPeligrosos)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(input, patron, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
