using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Pages.Tareas
{
    [Authorize(Policy = "OnlyJefe")]
    public class GestionarModel : PageModel
    {
        private readonly ProyectoApiClient _proyectoApi;
        private readonly TareaApiClient _tareaApi;
        private readonly UsuarioApiClient _usuarioApi;
        private readonly SagaApiClient _sagaApi;
        private readonly Sistema_de_Gestion_de_Proyectos_y_Tareas.Services.ReporteService _reporteService;
        private readonly ILogger<GestionarModel> _logger;

        public GestionarModel(
            ProyectoApiClient proyectoApi,
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            SagaApiClient sagaApi,
            Sistema_de_Gestion_de_Proyectos_y_Tareas.Services.ReporteService reporteService,
            ILogger<GestionarModel> logger)
        {
            _proyectoApi = proyectoApi;
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
            _sagaApi = sagaApi;
            _reporteService = reporteService;
            _logger = logger;
        }

        public List<ProyectoDTO> Proyectos { get; set; } = new();
        public List<TareaDTO> Tareas { get; set; } = new();
        public List<UsuarioDTO> EmpleadosDisponibles { get; set; } = new();
        public Dictionary<int, List<UsuarioDTO>> EmpleadosAsignados { get; set; } = new();

        [BindProperty]
        public int TareaId { get; set; }

        [BindProperty]
        public string EmpleadosIds { get; set; } = "[]";

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar todos los proyectos
                Proyectos = (await _proyectoApi.GetAllAsync())?.ToList() ?? new();

                if (!Proyectos.Any())
                {
                    TempData["WarningMessage"] = "?? No hay proyectos disponibles en el sistema.";
                }

                // Cargar todas las tareas
                Tareas = (await _tareaApi.GetAllAsync())?.ToList() ?? new();

                // Cargar todos los usuarios
                var todosLosUsuarios = (await _usuarioApi.GetAllAsync())?.ToList() ?? new();

                // Cargar empleados asignados a cada tarea
                var todosLosEmpleadosAsignadosIds = new HashSet<int>();
                foreach (var tarea in Tareas)
                {
                    var asignadosIds = await _tareaApi.GetUsuariosAsignadosAsync(tarea.Id);
                    var asignados = todosLosUsuarios
                        .Where(u => asignadosIds.Contains(u.Id))
                        .ToList();
                    
                    EmpleadosAsignados[tarea.Id] = asignados;
                    
                    // Recopilar todos los IDs de empleados asignados a cualquier tarea
                    foreach (var id in asignadosIds)
                    {
                        todosLosEmpleadosAsignadosIds.Add(id);
                    }
                }

                // ✅ CORREGIDO: Filtrar empleados disponibles basándose en asignaciones REALES
                // en lugar de confiar solo en la columna disponible_tarea
                EmpleadosDisponibles = todosLosUsuarios
                    .Where(u => u.Rol == "Empleado" && 
                                u.Estado == 1 && 
                                !todosLosEmpleadosAsignadosIds.Contains(u.Id)) // No está asignado a ninguna tarea
                    .ToList();

                _logger.LogInformation($"Empleados disponibles (sin tarea activa): {EmpleadosDisponibles.Count}");
                _logger.LogInformation($"Empleados ocupados en tareas: {todosLosEmpleadosAsignadosIds.Count}");

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar datos en Gestionar: {ex.Message}");
                TempData["ErrorMessage"] = "? Error al cargar los datos del sistema.";
                return RedirectToPage("/Tareas/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (TareaId <= 0)
                {
                    TempData["ErrorMessage"] = "? Debe seleccionar una tarea válida.";
                    return RedirectToPage();
                }

                // Deserializar los IDs de empleados
                List<int> empleadosIdsLista;
                try
                {
                    empleadosIdsLista = JsonSerializer.Deserialize<List<int>>(EmpleadosIds) ?? new();
                }
                catch
                {
                    TempData["ErrorMessage"] = "❌ Error al procesar los empleados seleccionados.";
                    return RedirectToPage();
                }

                // ✅ CORREGIDO: Permitir lista vacía para desasignar todos los empleados
                _logger.LogInformation($"Procesando asignación: {empleadosIdsLista.Count} empleados para tarea {TareaId}");

                // Obtener información de la tarea para la Saga
                var tarea = await _tareaApi.GetByIdAsync(TareaId);
                
                if (tarea == null)
                {
                    TempData["ErrorMessage"] = "? La tarea seleccionada no existe.";
                    return RedirectToPage();
                }

                // ============================================================
                // ?? USAR SAGA PARA ASIGNAR EMPLEADOS
                // ============================================================
                // Nota: Como la tarea ya existe, solo asignamos empleados
                // directamente sin crear una nueva tarea

                // Opción 1: Asignación directa (sin Saga, ya que la tarea existe)
                var dto = new AsignarUsuariosDTO
                {
                    TareaId = TareaId,
                    UsuariosIds = empleadosIdsLista
                };

                var resultado = await _tareaApi.AsignarUsuariosAsync(dto);

                if (!resultado)
                {
                    TempData["ErrorMessage"] = "❌ Error al asignar empleados. Algunos empleados pueden no estar disponibles.";
                    return RedirectToPage();
                }

                // TODO: Llamar al endpoint de usuario para marcar como ocupados
                // Por ahora el backend lo manejará

                // ✅ Mensaje diferenciado según la acción
                if (empleadosIdsLista.Count == 0)
                {
                    TempData["SuccessMessage"] = "✅ Todos los empleados han sido desasignados de la tarea.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"✅ {empleadosIdsLista.Count} empleado(s) asignado(s) exitosamente a la tarea.";
                }
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al asignar empleados: {ex.Message}");
                TempData["ErrorMessage"] = $"? Error al asignar empleados: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ============================================================
        // MÉTODOS PARA GENERACIÓN DE REPORTES
        // ============================================================

        public async Task<IActionResult> OnGetGenerarReporteAsync(int tareaId, string formato)
        {
            try
            {
                if (tareaId <= 0)
                {
                    TempData["ErrorMessage"] = "? Debe especificar una tarea válida.";
                    return RedirectToPage();
                }

                // Obtener nombre del usuario actual
                var usuarioNombre = User.Identity?.Name ?? "Sistema";

                byte[] fileBytes;
                string contentType;
                string fileName;

                if (formato.ToLower() == "pdf")
                {
                    _logger.LogInformation($"Generando reporte PDF para tarea {tareaId} por usuario {usuarioNombre}");
                    fileBytes = await _reporteService.GenerarReporteTareaPDFAsync(tareaId, usuarioNombre);
                    contentType = "application/pdf";
                    fileName = $"Reporte_Tarea_{tareaId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                }
                else if (formato.ToLower() == "excel")
                {
                    _logger.LogInformation($"Generando reporte Excel para tarea {tareaId} por usuario {usuarioNombre}");
                    fileBytes = await _reporteService.GenerarReporteTareaExcelAsync(tareaId, usuarioNombre);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"Reporte_Tarea_{tareaId}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }
                else
                {
                    TempData["ErrorMessage"] = "? Formato de reporte no válido.";
                    return RedirectToPage();
                }

                // ? AGREGAR HEADER PARA FORZAR DESCARGA
                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte: {ex.Message}");
                TempData["ErrorMessage"] = $"? Error al generar el reporte: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetGenerarReporteCompletoAsync(string formato)
        {
            try
            {
                // Obtener nombre del usuario actual
                var usuarioNombre = User.Identity?.Name ?? "Sistema";

                byte[] fileBytes;
                string contentType;
                string fileName;

                if (formato.ToLower() == "pdf")
                {
                    _logger.LogInformation($"Generando reporte completo PDF por usuario {usuarioNombre}");
                    fileBytes = await _reporteService.GenerarReporteCompletoPDFAsync(usuarioNombre);
                    contentType = "application/pdf";
                    fileName = $"Reporte_Completo_Sistema_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                }
                else if (formato.ToLower() == "excel")
                {
                    _logger.LogInformation($"Generando reporte completo Excel por usuario {usuarioNombre}");
                    fileBytes = await _reporteService.GenerarReporteCompletoExcelAsync(usuarioNombre);
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = $"Reporte_Completo_Sistema_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                }
                else
                {
                    TempData["ErrorMessage"] = "? Formato de reporte no válido.";
                    return RedirectToPage();
                }

                // ? AGREGAR HEADER PARA FORZAR DESCARGA
                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte completo: {ex.Message}");
                TempData["ErrorMessage"] = $"? Error al generar el reporte completo: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
