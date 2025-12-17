using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using System.Collections.Generic;
using System.IO;
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
                Proyectos = (await _proyectoApi.GetAllAsync())?.ToList() ?? new();

                if (!Proyectos.Any())
                {
                    TempData["WarningMessage"] = "No hay proyectos disponibles en el sistema.";
                }

                Tareas = (await _tareaApi.GetAllAsync())?.ToList() ?? new();

                var todosLosUsuarios = (await _usuarioApi.GetAllAsync())?.ToList() ?? new();

                var todosLosEmpleadosAsignadosIds = new HashSet<int>();
                foreach (var tarea in Tareas)
                {
                    var asignadosIds = await _tareaApi.GetUsuariosAsignadosAsync(tarea.Id);
                    var asignados = todosLosUsuarios
                        .Where(u => asignadosIds.Contains(u.Id))
                        .ToList();
                    
                    EmpleadosAsignados[tarea.Id] = asignados;
                    
                    foreach (var id in asignadosIds)
                    {
                        todosLosEmpleadosAsignadosIds.Add(id);
                    }
                }

                EmpleadosDisponibles = todosLosUsuarios
                    .Where(u => u.Rol == "Empleado" && 
                                u.Estado == 1 && 
                                !todosLosEmpleadosAsignadosIds.Contains(u.Id))
                    .ToList();

                _logger.LogInformation($"Empleados disponibles (sin tarea activa): {EmpleadosDisponibles.Count}");
                _logger.LogInformation($"Empleados ocupados en tareas: {todosLosEmpleadosAsignadosIds.Count}");

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al cargar datos en Gestionar: {ex.Message}");
                TempData["ErrorMessage"] = "Error al cargar los datos del sistema.";
                return RedirectToPage("/Tareas/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (TareaId <= 0)
                {
                    TempData["ErrorMessage"] = "Debe seleccionar una tarea válida.";
                    return RedirectToPage();
                }

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

                _logger.LogInformation($"Procesando asignación: {empleadosIdsLista.Count} empleados para tarea {TareaId}");

                var tarea = await _tareaApi.GetByIdAsync(TareaId);
                
                if (tarea == null)
                {
                    TempData["ErrorMessage"] = "La tarea seleccionada no existe.";
                    return RedirectToPage();
                }

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

                if (empleadosIdsLista.Count > 0)
                {
                    try
                    {
                        _logger.LogInformation($"📄 Generando reportes para descarga automática (tarea {TareaId})");
                        
                        var nombreUsuario = User.Identity?.Name ?? "Sistema";
                        
                        var directorioActual = Directory.GetCurrentDirectory();
                        var directorioRaiz = Directory.GetParent(directorioActual)?.Parent?.FullName;
                        
                        if (string.IsNullOrEmpty(directorioRaiz))
                        {
                            directorioRaiz = directorioActual;
                        }
                        
                        var rutaReportes = Path.Combine(directorioRaiz, "reportes");
                        Directory.CreateDirectory(rutaReportes);

                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        
                        var pdfBytes = await _reporteService.GenerarReporteTareaPDFAsync(TareaId, nombreUsuario);
                        var nombreArchivoPdf = $"Tarea_{TareaId}_{timestamp}.pdf";
                        var rutaCompletaPdf = Path.Combine(rutaReportes, nombreArchivoPdf);
                        await System.IO.File.WriteAllBytesAsync(rutaCompletaPdf, pdfBytes);
                        _logger.LogInformation($"✅ Reporte PDF guardado en: {rutaCompletaPdf}");
                        
                        var excelBytes = await _reporteService.GenerarReporteTareaExcelAsync(TareaId, nombreUsuario);
                        var nombreArchivoExcel = $"Tarea_{TareaId}_{timestamp}.xlsx";
                        var rutaCompletaExcel = Path.Combine(rutaReportes, nombreArchivoExcel);
                        await System.IO.File.WriteAllBytesAsync(rutaCompletaExcel, excelBytes);
                        _logger.LogInformation($"✅ Reporte Excel guardado en: {rutaCompletaExcel}");

                        TempData["SuccessMessage"] = $"✅ {empleadosIdsLista.Count} empleado(s) asignado(s) exitosamente. 📄 Reportes guardados: {nombreArchivoPdf} y {nombreArchivoExcel}";
                        TempData["DownloadPdf"] = nombreArchivoPdf;
                        TempData["DownloadExcel"] = nombreArchivoExcel;
                    }
                    catch (Exception exReporte)
                    {
                        _logger.LogWarning($"⚠️ No se pudieron generar los reportes: {exReporte.Message}");
                        TempData["SuccessMessage"] = $"✅ {empleadosIdsLista.Count} empleado(s) asignado(s) exitosamente. ⚠️ Los reportes no pudieron generarse.";
                    }
                }
                else
                {
                    TempData["SuccessMessage"] = "✅ Todos los empleados han sido desasignados de la tarea.";
                }
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al asignar empleados: {ex.Message}");
                TempData["ErrorMessage"] = $"Error al asignar empleados: {ex.Message}";
                return RedirectToPage();
            }
        }


        public async Task<IActionResult> OnGetGenerarReporteAsync(int tareaId, string formato)
        {
            try
            {
                if (tareaId <= 0)
                {
                    TempData["ErrorMessage"] = "Debe especificar una tarea válida.";
                    return RedirectToPage();
                }

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
                    TempData["ErrorMessage"] = "Formato de reporte no válido.";
                    return RedirectToPage();
                }

                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte: {ex.Message}");
                TempData["ErrorMessage"] = $"Error al generar el reporte: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetDescargarReporteAsync(string archivo)
        {
            try
            {
                var directorioActual = Directory.GetCurrentDirectory();
                var directorioRaiz = Directory.GetParent(directorioActual)?.Parent?.FullName;
                
                if (string.IsNullOrEmpty(directorioRaiz))
                {
                    directorioRaiz = directorioActual;
                }
                
                var rutaReportes = Path.Combine(directorioRaiz, "reportes");
                var rutaArchivo = Path.Combine(rutaReportes, archivo);

                if (!System.IO.File.Exists(rutaArchivo))
                {
                    TempData["ErrorMessage"] = "El archivo no existe.";
                    return RedirectToPage();
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(rutaArchivo);
                var contentType = archivo.EndsWith(".pdf") ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{archivo}\"");
                
                return File(fileBytes, contentType, archivo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al descargar archivo: {ex.Message}");
                TempData["ErrorMessage"] = "Error al descargar el archivo.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetGenerarReporteCompletoAsync(string formato)
        {
            try
            {
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
                    TempData["ErrorMessage"] = "Formato de reporte no válido.";
                    return RedirectToPage();
                }

                Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte completo: {ex.Message}");
                TempData["ErrorMessage"] = $"Error al generar el reporte completo: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}
