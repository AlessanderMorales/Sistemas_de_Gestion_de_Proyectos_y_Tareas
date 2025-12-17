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
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly BaseColor _colorPrimario = new BaseColor(211, 47, 47);      // Rojo Volcánico
        private readonly BaseColor _colorSecundario = new BaseColor(255, 87, 34);    // Naranja
        private readonly BaseColor _colorTexto = new BaseColor(64, 64, 64);          // Texto oscuro

        public ReporteService(
            TareaApiClient tareaApi,
            UsuarioApiClient usuarioApi,
            ProyectoApiClient proyectoApi,
            ILogger<ReporteService> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _tareaApi = tareaApi;
            _usuarioApi = usuarioApi;
            _proyectoApi = proyectoApi;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

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
                    var document = new Document(PageSize.A4, 40, 40, 40, 40);
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    
                    document.Open();

                    try
                    {
                        var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Logo.png");
                        
                        if (File.Exists(logoPath))
                        {
                            var logo = Image.GetInstance(logoPath);
                            logo.ScaleToFit(80f, 80f);
                            
                            var headerTable = new PdfPTable(2);
                            headerTable.WidthPercentage = 100;
                            headerTable.SetWidths(new float[] { 1f, 4f });
                            
                            var logoCell = new PdfPCell(logo);
                            logoCell.Border = Rectangle.NO_BORDER;
                            logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            logoCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            headerTable.AddCell(logoCell);
                            
                            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, _colorPrimario);
                            var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, _colorSecundario);
                            
                            var titlePhrase = new Phrase();
                            titlePhrase.Add(new Chunk("SISTEMA DE GESTIÓN\n", titleFont));
                            titlePhrase.Add(new Chunk("Reporte de Tarea", subtitleFont));
                            
                            var titleCell = new PdfPCell(titlePhrase);
                            titleCell.Border = Rectangle.NO_BORDER;
                            titleCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            titleCell.PaddingLeft = 10;
                            headerTable.AddCell(titleCell);
                            
                            document.Add(headerTable);
                        }
                        else
                        {
                            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, _colorPrimario);
                            var title = new Paragraph("SISTEMA DE GESTIÓN\n", titleFont);
                            title.Alignment = Element.ALIGN_CENTER;
                            document.Add(title);
                            
                            var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, _colorSecundario);
                            var subtitle = new Paragraph("Reporte de Tarea\n", subtitleFont);
                            subtitle.Alignment = Element.ALIGN_CENTER;
                            document.Add(subtitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al cargar logo: {ex.Message}");
                        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, _colorPrimario);
                        var title = new Paragraph("REPORTE DE TAREA\n\n", titleFont);
                        title.Alignment = Element.ALIGN_CENTER;
                        document.Add(title);
                    }

                    var lineSeparator = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(2f, 100f, _colorSecundario, Element.ALIGN_CENTER, -2)));
                    document.Add(lineSeparator);
                    document.Add(new Paragraph("\n"));

                    var headerInfoFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, _colorPrimario);
                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, _colorTexto);

                    var infoTable = new PdfPTable(2);
                    infoTable.WidthPercentage = 100;
                    infoTable.SetWidths(new float[] { 1f, 2f });
                    infoTable.SpacingBefore = 10f;
                    infoTable.SpacingAfter = 15f;

                    AddInfoRow(infoTable, "Proyecto:", proyecto?.Nombre ?? "N/A", headerInfoFont, normalFont);
                    AddInfoRow(infoTable, "Tarea:", tarea.Titulo, headerInfoFont, normalFont);
                    AddInfoRow(infoTable, "Descripción:", tarea.Descripcion ?? "Sin descripción", headerInfoFont, normalFont);
                    AddInfoRow(infoTable, "Estado:", tarea.Status, headerInfoFont, normalFont);
                    AddInfoRow(infoTable, "Prioridad:", tarea.Prioridad, headerInfoFont, normalFont);
                    AddInfoRow(infoTable, "Fecha:", tarea.FechaRegistro.ToString("dd/MM/yyyy"), headerInfoFont, normalFont);
                    AddInfoRow(infoTable, "Generado por:", usuarioNombre, headerInfoFont, normalFont);

                    document.Add(infoTable);

                    var sectionTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.White);
                    
                    var sectionHeader = new PdfPTable(1);
                    sectionHeader.WidthPercentage = 100;
                    var sectionCell = new PdfPCell(new Phrase("EMPLEADOS ASIGNADOS", sectionTitleFont));
                    sectionCell.BackgroundColor = _colorPrimario;
                    sectionCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    sectionCell.Padding = 8;
                    sectionCell.Border = Rectangle.NO_BORDER;
                    sectionHeader.AddCell(sectionCell);
                    document.Add(sectionHeader);

                    var empleadosTable = new PdfPTable(4);
                    empleadosTable.WidthPercentage = 100;
                    empleadosTable.SetWidths(new float[] { 0.5f, 2f, 2f, 2.5f });

                    var tableHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.White);
                    AddTableHeader(empleadosTable, "#", tableHeaderFont, _colorSecundario);
                    AddTableHeader(empleadosTable, "Nombre", tableHeaderFont, _colorSecundario);
                    AddTableHeader(empleadosTable, "Apellido", tableHeaderFont, _colorSecundario);
                    AddTableHeader(empleadosTable, "Email", tableHeaderFont, _colorSecundario);

                    var tableDataFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, _colorTexto);
                    if (empleadosAsignados.Any())
                    {
                        int contador = 1;
                        foreach (var emp in empleadosAsignados)
                        {
                            var bgColor = contador % 2 == 0 ? new BaseColor(245, 245, 245) : BaseColor.White;
                            
                            AddTableCell(empleadosTable, contador.ToString(), tableDataFont, bgColor, Element.ALIGN_CENTER);
                            AddTableCell(empleadosTable, emp.Nombres, tableDataFont, bgColor, Element.ALIGN_LEFT);
                            AddTableCell(empleadosTable, emp.PrimerApellido, tableDataFont, bgColor, Element.ALIGN_LEFT);
                            AddTableCell(empleadosTable, emp.Email, tableDataFont, bgColor, Element.ALIGN_LEFT);
                            contador++;
                        }
                    }
                    else
                    {
                        var emptyCell = new PdfPCell(new Phrase("No hay empleados asignados", tableDataFont));
                        emptyCell.Colspan = 4;
                        emptyCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        emptyCell.Padding = 15;
                        empleadosTable.AddCell(emptyCell);
                    }

                    document.Add(empleadosTable);

                    document.Add(new Paragraph("\n"));
                    
                    var summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 50;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    
                    var summaryLabelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, _colorTexto);
                    var summaryValueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, _colorPrimario);
                    
                    var labelCell = new PdfPCell(new Phrase("Total Empleados:", summaryLabelFont));
                    labelCell.Border = Rectangle.TOP_BORDER;
                    labelCell.BorderColor = _colorSecundario;
                    labelCell.BorderWidth = 2;
                    labelCell.Padding = 10;
                    labelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.AddCell(labelCell);
                    
                    var valueCell = new PdfPCell(new Phrase(empleadosAsignados.Count.ToString(), summaryValueFont));
                    valueCell.Border = Rectangle.TOP_BORDER;
                    valueCell.BorderColor = _colorSecundario;
                    valueCell.BorderWidth = 2;
                    valueCell.Padding = 10;
                    valueCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    valueCell.BackgroundColor = new BaseColor(255, 243, 224);
                    summaryTable.AddCell(valueCell);
                    
                    document.Add(summaryTable);

                    // Pie de página
                    document.Add(new Paragraph("\n\n"));
                    var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, new BaseColor(128, 128, 128));
                    var footer = new Paragraph();
                    footer.Add(new Chunk("─────────────────────────────────────────────────────────────\n", footerFont));
                    footer.Add(new Chunk($"Generado por Sistema de Gestión | {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Usuario: {usuarioNombre}", footerFont));
                    footer.Alignment = Element.ALIGN_CENTER;
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

        private void AddInfoRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            var labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.PaddingBottom = 5;
            table.AddCell(labelCell);
            
            var valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.PaddingBottom = 5;
            table.AddCell(valueCell);
        }

        private void AddTableHeader(PdfPTable table, string text, Font font, BaseColor bgColor)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = bgColor;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 8;
            cell.BorderColor = BaseColor.White;
            table.AddCell(cell);
        }

        private void AddTableCell(PdfPTable table, string text, Font font, BaseColor bgColor, int alignment)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = bgColor;
            cell.HorizontalAlignment = alignment;
            cell.Padding = 6;
            cell.BorderColor = new BaseColor(220, 220, 220);
            table.AddCell(cell);
        }

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

                    worksheet.Cell(1, 1).Value = "SISTEMA DE GESTIÓN DE PROYECTOS Y TAREAS";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#D32F2F");
                    worksheet.Range(1, 1, 1, 4).Merge();

                    worksheet.Cell(2, 1).Value = "Reporte de Asignación de Tarea";
                    worksheet.Cell(2, 1).Style.Font.FontSize = 12;
                    worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#FF5722");
                    worksheet.Range(2, 1, 2, 4).Merge();

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

                    worksheet.Cell(row, 1).Value = "EMPLEADOS ASIGNADOS:";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontSize = 14;
                    worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                    worksheet.Range(row, 1, row, 4).Merge();
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#D32F2F");
                    row++;

                    worksheet.Cell(row, 1).Value = "#";
                    worksheet.Cell(row, 2).Value = "Nombre";
                    worksheet.Cell(row, 3).Value = "Apellido";
                    worksheet.Cell(row, 4).Value = "Email";
                    worksheet.Range(row, 1, row, 4).Style.Font.Bold = true;
                    worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF5722");
                    worksheet.Range(row, 1, row, 4).Style.Font.FontColor = XLColor.White;
                    row++;

                    if (empleadosAsignados.Any())
                    {
                        int contador = 1;
                        foreach (var emp in empleadosAsignados)
                        {
                            worksheet.Cell(row, 1).Value = contador;
                            worksheet.Cell(row, 2).Value = emp.Nombres;
                            worksheet.Cell(row, 3).Value = emp.PrimerApellido;
                            worksheet.Cell(row, 4).Value = emp.Email;
                            
                            if (contador % 2 == 0)
                                worksheet.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3E0");
                            
                            contador++;
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
                    worksheet.Cell(row, 3).Value = "Total Empleados:";
                    worksheet.Cell(row, 3).Style.Font.Bold = true;
                    worksheet.Cell(row, 4).Value = empleadosAsignados.Count;
                    worksheet.Cell(row, 4).Style.Font.Bold = true;
                    worksheet.Cell(row, 4).Style.Font.FontColor = XLColor.FromHtml("#D32F2F");
                    worksheet.Cell(row, 4).Style.Font.FontSize = 14;

                    row += 2;
                    worksheet.Cell(row, 1).Value = $"Reporte generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;
                    row++;
                    worksheet.Cell(row, 1).Value = $"Generado por: {usuarioNombre}";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;

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

        public async Task<byte[]> GenerarReporteCompletoPDFAsync(string usuarioNombre = "Sistema")
        {
            try
            {
                var proyectos = await _proyectoApi.GetAllAsync();
                var tareas = await _tareaApi.GetAllAsync();
                var usuarios = await _usuarioApi.GetAllAsync();

                using (var memoryStream = new MemoryStream())
                {
                    var document = new Document(PageSize.A4, 40, 40, 40, 40);
                    var writer = PdfWriter.GetInstance(document, memoryStream);
                    
                    document.Open();

                    try
                    {
                        var logoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Image", "Logo.png");
                        
                        if (File.Exists(logoPath))
                        {
                            var logo = Image.GetInstance(logoPath);
                            logo.ScaleToFit(80f, 80f);
                            
                            var headerTable = new PdfPTable(2);
                            headerTable.WidthPercentage = 100;
                            headerTable.SetWidths(new float[] { 1f, 4f });
                            
                            var logoCell = new PdfPCell(logo);
                            logoCell.Border = Rectangle.NO_BORDER;
                            logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            logoCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            headerTable.AddCell(logoCell);
                            
                            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, _colorPrimario);
                            var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, _colorSecundario);
                            
                            var titlePhrase = new Phrase();
                            titlePhrase.Add(new Chunk("SISTEMA DE GESTIÓN\n", titleFont));
                            titlePhrase.Add(new Chunk("Reporte Completo del Sistema", subtitleFont));
                            
                            var titleCell = new PdfPCell(titlePhrase);
                            titleCell.Border = Rectangle.NO_BORDER;
                            titleCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            titleCell.PaddingLeft = 10;
                            headerTable.AddCell(titleCell);
                            
                            document.Add(headerTable);
                        }
                        else
                        {
                            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, _colorPrimario);
                            var title = new Paragraph("SISTEMA DE GESTIÓN\n", titleFont);
                            title.Alignment = Element.ALIGN_CENTER;
                            document.Add(title);
                            
                            var subtitleFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, _colorSecundario);
                            var subtitle = new Paragraph("Reporte Completo del Sistema\n", subtitleFont);
                            subtitle.Alignment = Element.ALIGN_CENTER;
                            document.Add(subtitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al cargar logo: {ex.Message}");
                        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, _colorPrimario);
                        var title = new Paragraph("REPORTE COMPLETO DEL SISTEMA\n\n", titleFont);
                        title.Alignment = Element.ALIGN_CENTER;
                        document.Add(title);
                    }

                    var lineSeparator = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(2f, 100f, _colorSecundario, Element.ALIGN_CENTER, -2)));
                    document.Add(lineSeparator);
                    document.Add(new Paragraph("\n"));

                    var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, _colorTexto);
                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, _colorPrimario);
                    var sectionHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.White);

                    var resumenHeader = new PdfPTable(1);
                    resumenHeader.WidthPercentage = 100;
                    var resumenCell = new PdfPCell(new Phrase("RESUMEN GENERAL", sectionHeaderFont));
                    resumenCell.BackgroundColor = _colorPrimario;
                    resumenCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    resumenCell.Padding = 8;
                    resumenCell.Border = Rectangle.NO_BORDER;
                    resumenHeader.AddCell(resumenCell);
                    document.Add(resumenHeader);
                    
                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph($"Total de Proyectos: {proyectos.Count}", normalFont));
                    document.Add(new Paragraph($"Total de Tareas: {tareas.Count}", normalFont));
                    document.Add(new Paragraph($"Total de Usuarios: {usuarios.Count}", normalFont));
                    document.Add(new Paragraph($"Empleados: {usuarios.Count(u => u.Rol == "Empleado")}", normalFont));
                    document.Add(new Paragraph($"Jefes de Proyecto: {usuarios.Count(u => u.Rol == "JefeDeProyecto")}", normalFont));
                    document.Add(new Paragraph("\n\n"));

                    var proyectosHeader = new PdfPTable(1);
                    proyectosHeader.WidthPercentage = 100;
                    var proyectosCell = new PdfPCell(new Phrase("PROYECTOS", sectionHeaderFont));
                    proyectosCell.BackgroundColor = _colorSecundario;
                    proyectosCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    proyectosCell.Padding = 8;
                    proyectosCell.Border = Rectangle.NO_BORDER;
                    proyectosHeader.AddCell(proyectosCell);
                    document.Add(proyectosHeader);
                    
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

                    // SECCIÓN DE USUARIOS
                    document.Add(new Paragraph("\n"));
                    var usuariosHeader = new PdfPTable(1);
                    usuariosHeader.WidthPercentage = 100;
                    var usuariosCell = new PdfPCell(new Phrase("USUARIOS", sectionHeaderFont));
                    usuariosCell.BackgroundColor = new BaseColor(76, 175, 80); // Verde
                    usuariosCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    usuariosCell.Padding = 8;
                    usuariosCell.Border = Rectangle.NO_BORDER;
                    usuariosHeader.AddCell(usuariosCell);
                    document.Add(usuariosHeader);
                    
                    document.Add(new Paragraph("\n"));

                    // Obtener asignaciones de usuarios
                    var usuariosConAsignaciones = new List<(UsuarioDTO usuario, List<string> proyectos, List<string> tareas)>();
                    
                    foreach (var usuario in usuarios.Where(u => u.Rol == "Empleado" || u.Rol == "JefeDeProyecto"))
                    {
                        var proyectosAsignados = new List<string>();
                        var tareasAsignadas = new List<string>();

                        // Buscar tareas asignadas al usuario
                        foreach (var tarea in tareas)
                        {
                            try
                            {
                                var empleadosIds = await _tareaApi.GetUsuariosAsignadosAsync(tarea.Id);
                                if (empleadosIds.Contains(usuario.Id))
                                {
                                    tareasAsignadas.Add(tarea.Titulo);
                                    
                                    // Agregar proyecto asociado
                                    var proyecto = proyectos.FirstOrDefault(p => p.IdProyecto == tarea.IdProyecto);
                                    if (proyecto != null && !proyectosAsignados.Contains(proyecto.Nombre))
                                    {
                                        proyectosAsignados.Add(proyecto.Nombre);
                                    }
                                }
                            }
                            catch { }
                        }

                        usuariosConAsignaciones.Add((usuario, proyectosAsignados, tareasAsignadas));
                    }

                    // Mostrar usuarios con asignaciones
                    var usuariosConTareas = usuariosConAsignaciones.Where(u => u.tareas.Any()).ToList();
                    var usuariosSinTareas = usuariosConAsignaciones.Where(u => !u.tareas.Any()).ToList();

                    if (usuariosConTareas.Any())
                    {
                        document.Add(new Paragraph("Usuarios con Asignaciones:", boldFont));
                        document.Add(new Paragraph("\n"));

                        foreach (var (usuario, proyectosAsig, tareasAsig) in usuariosConTareas)
                        {
                            document.Add(new Paragraph($"• {usuario.Nombres} {usuario.PrimerApellido} ({usuario.Rol})", boldFont));
                            
                            if (proyectosAsig.Any())
                            {
                                document.Add(new Paragraph($"  - Proyectos: {string.Join(", ", proyectosAsig)}", normalFont));
                            }
                            
                            if (tareasAsig.Any())
                            {
                                document.Add(new Paragraph($"  - Tareas ({tareasAsig.Count}): {string.Join(", ", tareasAsig.Take(3))}{(tareasAsig.Count > 3 ? "..." : "")}", normalFont));
                            }
                            
                            document.Add(new Paragraph("\n"));
                        }
                    }

                    if (usuariosSinTareas.Any())
                    {
                        document.Add(new Paragraph("\n"));
                        document.Add(new Paragraph("Usuarios Sin Asignaciones:", boldFont));
                        document.Add(new Paragraph("\n"));

                        foreach (var (usuario, _, _) in usuariosSinTareas)
                        {
                            document.Add(new Paragraph($"• {usuario.Nombres} {usuario.PrimerApellido} ({usuario.Rol}) - Disponible", normalFont));
                        }
                        document.Add(new Paragraph("\n"));
                    }

                    document.Add(new Paragraph("\n\n"));
                    var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, new BaseColor(128, 128, 128));
                    var footer = new Paragraph();
                    footer.Add(new Chunk("─────────────────────────────────────────────────────────────\n", footerFont));
                    footer.Add(new Chunk($"Generado por Sistema de Gestión | {DateTime.Now:dd/MM/yyyy HH:mm:ss} | Usuario: {usuarioNombre}", footerFont));
                    footer.Alignment = Element.ALIGN_CENTER;
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

        public async Task<byte[]> GenerarReporteCompletoExcelAsync(string usuarioNombre = "Sistema")
        {
            try
            {
                var proyectos = await _proyectoApi.GetAllAsync();
                var tareas = await _tareaApi.GetAllAsync();
                var usuarios = await _usuarioApi.GetAllAsync();

                using (var workbook = new XLWorkbook())
                {
                    var resumenSheet = workbook.Worksheets.Add("Resumen");
                    
                    resumenSheet.Cell(1, 1).Value = "SISTEMA DE GESTIÓN DE PROYECTOS Y TAREAS";
                    resumenSheet.Cell(1, 1).Style.Font.Bold = true;
                    resumenSheet.Cell(1, 1).Style.Font.FontSize = 16;
                    resumenSheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#D32F2F");
                    resumenSheet.Range(1, 1, 1, 4).Merge();
                    
                    resumenSheet.Cell(2, 1).Value = "Reporte Completo del Sistema";
                    resumenSheet.Cell(2, 1).Style.Font.FontSize = 12;
                    resumenSheet.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#FF5722");
                    resumenSheet.Range(2, 1, 2, 4).Merge();

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

                    resumenSheet.Cell(row, 1).Value = "Reporte generado el:";
                    resumenSheet.Cell(row, 1).Style.Font.Italic = true;
                    resumenSheet.Cell(row, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    row++;
                    resumenSheet.Cell(row, 1).Value = "Generado por:";
                    resumenSheet.Cell(row, 1).Style.Font.Italic = true;
                    resumenSheet.Cell(row, 2).Value = usuarioNombre;

                    resumenSheet.Columns().AdjustToContents();

                    var proyectosSheet = workbook.Worksheets.Add("Proyectos");
                    
                    proyectosSheet.Cell(1, 1).Value = "Nombre";
                    proyectosSheet.Cell(1, 2).Value = "Descripción";
                    proyectosSheet.Cell(1, 3).Value = "Fecha Inicio";
                    proyectosSheet.Cell(1, 4).Value = "Fecha Fin";
                    proyectosSheet.Cell(1, 5).Value = "# Tareas";
                    proyectosSheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                    proyectosSheet.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF5722");
                    proyectosSheet.Range(1, 1, 1, 5).Style.Font.FontColor = XLColor.White;

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

                    var tareasSheet = workbook.Worksheets.Add("Tareas");
                    
                    tareasSheet.Cell(1, 1).Value = "Título";
                    tareasSheet.Cell(1, 2).Value = "Proyecto";
                    tareasSheet.Cell(1, 3).Value = "Estado";
                    tareasSheet.Cell(1, 4).Value = "Prioridad";
                    tareasSheet.Cell(1, 5).Value = "Fecha Creación";
                    tareasSheet.Range(1, 1, 1, 5).Style.Font.Bold = true;
                    tareasSheet.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FF5722");
                    tareasSheet.Range(1, 1, 1, 5).Style.Font.FontColor = XLColor.White;

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

                    // Hoja 4: Usuarios con colores
                    var usuariosSheet = workbook.Worksheets.Add("Usuarios");
                    
                    usuariosSheet.Cell(1, 1).Value = "Nombre";
                    usuariosSheet.Cell(1, 2).Value = "Apellido";
                    usuariosSheet.Cell(1, 3).Value = "Email";
                    usuariosSheet.Cell(1, 4).Value = "Rol";
                    usuariosSheet.Cell(1, 5).Value = "Proyectos";
                    usuariosSheet.Cell(1, 6).Value = "Tareas Asignadas";
                    usuariosSheet.Cell(1, 7).Value = "Estado";
                    usuariosSheet.Range(1, 1, 1, 7).Style.Font.Bold = true;
                    usuariosSheet.Range(1, 1, 1, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50"); // Verde
                    usuariosSheet.Range(1, 1, 1, 7).Style.Font.FontColor = XLColor.White;

                    row = 2;
                    foreach (var usuario in usuarios.Where(u => u.Rol == "Empleado" || u.Rol == "JefeDeProyecto"))
                    {
                        var proyectosAsignados = new List<string>();
                        var tareasAsignadas = new List<string>();

                        // Buscar tareas asignadas al usuario
                        foreach (var tarea in tareas)
                        {
                            try
                            {
                                var empleadosIds = await _tareaApi.GetUsuariosAsignadosAsync(tarea.Id);
                                if (empleadosIds.Contains(usuario.Id))
                                {
                                    tareasAsignadas.Add(tarea.Titulo);
                                    
                                    // Agregar proyecto asociado
                                    var proyecto = proyectos.FirstOrDefault(p => p.IdProyecto == tarea.IdProyecto);
                                    if (proyecto != null && !proyectosAsignados.Contains(proyecto.Nombre))
                                    {
                                        proyectosAsignados.Add(proyecto.Nombre);
                                    }
                                }
                            }
                            catch { }
                        }

                        usuariosSheet.Cell(row, 1).Value = usuario.Nombres;
                        usuariosSheet.Cell(row, 2).Value = usuario.PrimerApellido;
                        usuariosSheet.Cell(row, 3).Value = usuario.Email;
                        usuariosSheet.Cell(row, 4).Value = usuario.Rol;
                        usuariosSheet.Cell(row, 5).Value = proyectosAsignados.Any() ? string.Join(", ", proyectosAsignados) : "Ninguno";
                        usuariosSheet.Cell(row, 6).Value = tareasAsignadas.Any() ? $"{tareasAsignadas.Count} tarea(s)" : "Sin tareas";
                        usuariosSheet.Cell(row, 7).Value = tareasAsignadas.Any() ? "Ocupado" : "Disponible";
                        
                        // Color de fondo según estado
                        if (!tareasAsignadas.Any())
                        {
                            usuariosSheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9"); // Verde claro
                        }
                        else if (row % 2 == 0)
                        {
                            usuariosSheet.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF3E0");
                        }
                        
                        row++;
                    }

                    usuariosSheet.Columns().AdjustToContents();

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
