using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Tareas;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Usuarios;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.DTO.Proyectos;
using System.IO;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Services
{
    public class ReporteService
    {
        private readonly TareaApiClient _tareaApi;
        private readonly UsuarioApiClient _usuarioApi;
        private readonly ProyectoApiClient _proyectoApi;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi,
            ILogger<ReporteService> logger)
        {
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
            _proyectoApi = proyectoApi;
            _logger = logger;
        }

        // ============================================================
        // REPORTE DE TAREA INDIVIDUAL (PDF)
        // ============================================================
        public async Task<byte[]> GenerarReporteTareaPDFAsync(int tareaId, string usuarioNombre = "Sistema")
        {
            try
            {
                var tarea = await _tareaApi.GetByIdAsync(tareaId);
                if (tarea == null)
                    throw new Exception("Tarea no encontrada");

                var proyecto = await _proyectoApi.GetByIdAsync(tarea.IdProyecto ?? 0);
                var empleadosIds = await _tareaApi.GetUsuariosAsignadosAsync(tareaId);
                var todosUsuarios = await _usuarioApi.GetAllAsync();
                var empleadosAsignados = todosUsuarios.Where(u => empleadosIds.Contains(u.Id)).ToList();

                using (var memoryStream = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 50, 50, 50, 50);
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    
                    document.Open();

                    // Título
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(64, 64, 64));
                    var title = new Paragraph("REPORTE DE TAREA\n\n", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);

                    // Información de la tarea
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);

                    document.Add(new Paragraph($"Proyecto: {proyecto?.Nombre ?? "N/A"}", boldFont));
                    document.Add(new Paragraph($"Tarea: {tarea.Titulo}", boldFont));
                    document.Add(new Paragraph($"Descripción: {tarea.Descripcion ?? "Sin descripción"}", normalFont));
                    document.Add(new Paragraph($"Estado: {tarea.Status}", normalFont));
                    document.Add(new Paragraph($"Prioridad: {tarea.Prioridad}", normalFont));
                    document.Add(new Paragraph($"Fecha de Creación: {tarea.FechaRegistro:dd/MM/yyyy}", normalFont));
                    document.Add(new Paragraph("\n"));

                    // Empleados asignados
                    document.Add(new Paragraph("EMPLEADOS ASIGNADOS ACTUALES:", boldFont));
                    document.Add(new Paragraph("\n"));

                    if (empleadosAsignados.Any())
                    {
                        foreach (var emp in empleadosAsignados)
                        {
                            document.Add(new Paragraph($"• {emp.Nombres} {emp.PrimerApellido} ({emp.Email})", normalFont));
                        }
                    }
                    else
                    {
                        document.Add(new Paragraph("No hay empleados asignados actualmente.", normalFont));
                    }

                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph($"Total de empleados asignados: {empleadosAsignados.Count}", boldFont));

                    // Pie de página
                    document.Add(new Paragraph("\n\n"));
                    var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(128, 128, 128));
                    var footer = new Paragraph($"Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}\nGenerado por: {usuarioNombre}", footerFont);
                    footer.Alignment = Element.ALIGN_RIGHT;
                    document.Add(footer);

                    document.Close();
                    writer.Close();

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte PDF de tarea {tareaId}: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // REPORTE DE TAREA INDIVIDUAL (EXCEL)
        // ============================================================
        public async Task<byte[]> GenerarReporteTareaExcelAsync(int tareaId, string usuarioNombre = "Sistema")
        {
            try
            {
                var tarea = await _tareaApi.GetByIdAsync(tareaId);
                if (tarea == null)
                    throw new Exception("Tarea no encontrada");

                var proyecto = await _proyectoApi.GetByIdAsync(tarea.IdProyecto ?? 0);
                var empleadosIds = await _tareaApi.GetUsuariosAsignadosAsync(tareaId);
                var todosUsuarios = await _usuarioApi.GetAllAsync();
                var empleadosAsignados = todosUsuarios.Where(u => empleadosIds.Contains(u.Id)).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Reporte de Tarea");

                    // Encabezado
                    worksheet.Cell(1, 1).Value = "REPORTE DE TAREA";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Range(1, 1, 1, 4).Merge();

                    // Información de la tarea
                    int row = 3;
                    worksheet.Cell(row, 1).Value = "Proyecto:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = proyecto?.Nombre ?? "N/A";
                    row++;

                    worksheet.Cell(row, 1).Value = "Tarea:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = tarea.Titulo;
                    row++;

                    worksheet.Cell(row, 1).Value = "Descripción:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = tarea.Descripcion ?? "Sin descripción";
                    row++;

                    worksheet.Cell(row, 1).Value = "Estado:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = tarea.Status;
                    row++;

                    worksheet.Cell(row, 1).Value = "Prioridad:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = tarea.Prioridad;
                    row++;

                    worksheet.Cell(row, 1).Value = "Fecha de Creación:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Value = tarea.FechaRegistro.ToString("dd/MM/yyyy");
                    row += 2;

                    // Empleados asignados
                    worksheet.Cell(row, 1).Value = "EMPLEADOS ASIGNADOS:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                    row++;

                    // Encabezados de tabla
                    worksheet.Cell(row, 1).Value = "Nombre";
                    worksheet.Cell(row, 2).Value = "Apellido";
                    worksheet.Cell(row, 3).Value = "Email";
                    worksheet.Cell(row, 4).Value = "Estado";
                    worksheet.Range(row, 1, row, 4).Style.Font.Bold = true;
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
                    row++;

                    if (empleadosAsignados.Any())
                    {
                        foreach (var emp in empleadosAsignados)
                        {
                            worksheet.Cell(row, 1).Value = emp.Nombres;
                            worksheet.Cell(row, 2).Value = emp.PrimerApellido;
                            worksheet.Cell(row, 3).Value = emp.Email;
                            worksheet.Cell(row, 4).Value = "Activo";
                            row++;
                        }
                    }
                    else
                    {
                        worksheet.Cell(row, 1).Value = "No hay empleados asignados";
                        worksheet.Range(row, 1, row, 4).Merge();
                        row++;
                    }

                    row++;
                    worksheet.Cell(row, 1).Value = $"Total de empleados: {empleadosAsignados.Count}";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;

                    // Pie de página con usuario
                    row += 2;
                    worksheet.Cell(row, 1).Value = $"Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;
                    row++;
                    worksheet.Cell(row, 1).Value = $"Generado por: {usuarioNombre}";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;

                    // Ajustar columnas
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte Excel de tarea {tareaId}: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // REPORTE COMPLETO DEL SISTEMA (PDF)
        // ============================================================
        public async Task<byte[]> GenerarReporteCompletoPDFAsync(string usuarioNombre = "Sistema")
        {
            try
            {
                var proyectos = await _proyectoApi.GetAllAsync();
                var tareas = await _tareaApi.GetAllAsync();
                var usuarios = await _usuarioApi.GetAllAsync();

                using (var memoryStream = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 50, 50, 50, 50);
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    
                    document.Open();

                    // Título
                    var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, new BaseColor(64, 64, 64));
                    var title = new Paragraph("REPORTE COMPLETO DEL SISTEMA\n\n", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);

                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

                    // Resumen General
                    document.Add(new Paragraph("RESUMEN GENERAL", subtitleFont));
                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph($"Total de Proyectos: {proyectos.Count}", normalFont));
                    document.Add(new Paragraph($"Total de Tareas: {tareas.Count}", normalFont));
                    document.Add(new Paragraph($"Total de Usuarios: {usuarios.Count}", normalFont));
                    document.Add(new Paragraph($"Empleados: {usuarios.Count(u => u.Rol == "Empleado")}", normalFont));
                    document.Add(new Paragraph($"Jefes de Proyecto: {usuarios.Count(u => u.Rol == "JefeDeProyecto")}", normalFont));
                    document.Add(new Paragraph("\n\n"));

                    // Proyectos
                    document.Add(new Paragraph("PROYECTOS", subtitleFont));
                    document.Add(new Paragraph("\n"));

                    foreach (var proyecto in proyectos)
                    {
                        var tareasDelProyecto = tareas.Where(t => t.IdProyecto == proyecto.IdProyecto).ToList();
                        
                        document.Add(new Paragraph($"• {proyecto.Nombre}", boldFont));
                        document.Add(new Paragraph($"  - Descripción: {proyecto.Descripcion}", normalFont));
                        document.Add(new Paragraph($"  - Tareas: {tareasDelProyecto.Count}", normalFont));
                        document.Add(new Paragraph($"  - Fecha Inicio: {proyecto.FechaInicio?.ToString("dd/MM/yyyy") ?? "N/A"}", normalFont));
                        document.Add(new Paragraph($"  - Fecha Fin: {proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? "N/A"}", normalFont));
                        document.Add(new Paragraph("\n"));
                    }

                    // Pie de página con usuario
                    document.Add(new Paragraph("\n\n"));
                    var footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(128, 128, 128));
                    var footer = new Paragraph($"Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}\nGenerado por: {usuarioNombre}", footerFont);
                    footer.Alignment = Element.ALIGN_RIGHT;
                    document.Add(footer);

                    document.Close();
                    writer.Close();

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte completo PDF: {ex.Message}");
                throw;
            }
        }

        // ============================================================
        // REPORTE COMPLETO DEL SISTEMA (EXCEL)
        // ============================================================
        public async Task<byte[]> GenerarReporteCompletoExcelAsync(string usuarioNombre = "Sistema")
        {
            try
            {
                var proyectos = await _proyectoApi.GetAllAsync();
                var tareas = await _tareaApi.GetAllAsync();
                var usuarios = await _usuarioApi.GetAllAsync();

                using (var workbook = new XLWorkbook())
                {
                    // Hoja 1: Resumen
                    var resumenSheet = workbook.Worksheets.Add("Resumen");
                    
                    resumenSheet.Cell(1, 1).Value = "REPORTE COMPLETO DEL SISTEMA";
                    resumenSheet.Cell(1, 1).Style.Font.Bold = true;
                    resumenSheet.Cell(1, 1).Style.Font.FontSize = 16;
                    resumenSheet.Range(1, 1, 1, 4).Merge();

                    int row = 3;
                    resumenSheet.Cell(row, 1).Value = "Total de Proyectos:";
                    resumenSheet.Cell(row, 2).Value = proyectos.Count;
                    row++;

                    resumenSheet.Cell(row, 1).Value = "Total de Tareas:";
                    resumenSheet.Cell(row, 2).Value = tareas.Count;
                    row++;

                    resumenSheet.Cell(row, 1).Value = "Total de Usuarios:";
                    resumenSheet.Cell(row, 2).Value = usuarios.Count;
                    row++;

                    resumenSheet.Cell(row, 1).Value = "Empleados:";
                    resumenSheet.Cell(row, 2).Value = usuarios.Count(u => u.Rol == "Empleado");
                    row++;

                    resumenSheet.Cell(row, 1).Value = "Jefes de Proyecto:";
                    resumenSheet.Cell(row, 2).Value = usuarios.Count(u => u.Rol == "JefeDeProyecto");
                    row += 2;

                    // Agregar info de generación
                    resumenSheet.Cell(row, 1).Value = "Reporte generado el:";
                    resumenSheet.Cell(row, 1).Style.Font.Italic = true;
                    resumenSheet.Cell(row, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    row++;
                    resumenSheet.Cell(row, 1).Value = "Generado por:";
                    resumenSheet.Cell(row, 1).Style.Font.Italic = true;
                    resumenSheet.Cell(row, 2).Value = usuarioNombre;

                    resumenSheet.Columns().AdjustToContents();

                    // Hoja 2: Proyectos
                    var proyectosSheet = workbook.Worksheets.Add("Proyectos");
                    
                    proyectosSheet.Cell(1, 1).Value = "Nombre";
                    proyectosSheet.Cell(1, 2).Value = "Descripción";
                    proyectosSheet.Cell(1, 3).Value = "Fecha Inicio";
                    proyectosSheet.Cell(1, 4).Value = "Fecha Fin";
                    proyectosSheet.Cell(1, 5).Value = "# Tareas";
                    proyectosSheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                    proyectosSheet.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

                    row = 2;
                    foreach (var proyecto in proyectos)
                    {
                        var tareasDelProyecto = tareas.Count(t => t.IdProyecto == proyecto.IdProyecto);
                        
                        proyectosSheet.Cell(row, 1).Value = proyecto.Nombre;
                        proyectosSheet.Cell(row, 2).Value = proyecto.Descripcion;
                        proyectosSheet.Cell(row, 3).Value = proyecto.FechaInicio?.ToString("dd/MM/yyyy") ?? "N/A";
                        proyectosSheet.Cell(row, 4).Value = proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? "N/A";
                        proyectosSheet.Cell(row, 5).Value = tareasDelProyecto;
                        row++;
                    }

                    proyectosSheet.Columns().AdjustToContents();

                    // Hoja 3: Tareas
                    var tareasSheet = workbook.Worksheets.Add("Tareas");
                    
                    tareasSheet.Cell(1, 1).Value = "Título";
                    tareasSheet.Cell(1, 2).Value = "Proyecto";
                    tareasSheet.Cell(1, 3).Value = "Estado";
                    tareasSheet.Cell(1, 4).Value = "Prioridad";
                    tareasSheet.Cell(1, 5).Value = "Fecha Creación";
                    tareasSheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                    tareasSheet.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.LightGray;

                    row = 2;
                    foreach (var tarea in tareas)
                    {
                        var proyecto = proyectos.FirstOrDefault(p => p.IdProyecto == tarea.IdProyecto);
                        
                        tareasSheet.Cell(row, 1).Value = tarea.Titulo;
                        tareasSheet.Cell(row, 2).Value = proyecto?.Nombre ?? "N/A";
                        tareasSheet.Cell(row, 3).Value = tarea.Status;
                        tareasSheet.Cell(row, 4).Value = tarea.Prioridad;
                        tareasSheet.Cell(row, 5).Value = tarea.FechaRegistro.ToString("dd/MM/yyyy");
                        row++;
                    }

                    tareasSheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar reporte completo Excel: {ex.Message}");
                throw;
            }
        }
    }
}
