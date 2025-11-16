using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Application.Facades;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Middleware;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// 🌎 CONFIGURACIÓN DE CULTURA
// =========================================================
var cultureInfo = new CultureInfo("es-ES");
cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
cultureInfo.DateTimeFormat.DateSeparator = "/";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new[] { cultureInfo };
    options.SupportedUICultures = new[] { cultureInfo };
});

// =========================================================
// 🔐 AUTENTICACIÓN Y AUTORIZACIÓN
// =========================================================
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "Sgpt.AuthCookie";
        options.LoginPath = "/Login/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SoloAdmin", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("OnlyJefeOrEmpleado", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("JefeDeProyecto") ||
            ctx.User.IsInRole("Empleado") ||
            ctx.User.IsInRole("SuperAdmin")));

    options.AddPolicy("OnlyJefe", policy =>
        policy.RequireRole("JefeDeProyecto"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// =========================================================
// 📡 REGISTRO DE API CLIENTS (NUEVA ARQUITECTURA)
// =========================================================
builder.Services.AddHttpClient<UsuarioApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:UsuarioApi"]);
});

builder.Services.AddHttpClient<ProyectoApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:ProyectoApi"]);
});

builder.Services.AddHttpClient<TareaApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:TareaApi"]);
});

builder.Services.AddHttpClient<ComentarioApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:ComentarioApi"]);
});

// =========================================================
// 🏢 FACADE NUEVO
// =========================================================
builder.Services.AddScoped<GestionProyectosFacade>();

// =========================================================
// 📄 SERVICIOS DE REPORTES (SIGUEN INTERNOS)
// =========================================================

// =========================================================
// 📘 RAZOR PAGES + POLÍTICAS DE ACCESO
// =========================================================
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Usuarios", "SoloAdmin");
    options.Conventions.AuthorizeFolder("/Proyectos", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizeFolder("/Tareas", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizeFolder("/Comentarios", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizePage("/Index", "OnlyJefeOrEmpleado");

    options.Conventions.AllowAnonymousToPage("/Login/Login");
    options.Conventions.AllowAnonymousToPage("/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Logout");
});

var app = builder.Build();

// =========================================================
// 🚀 MIDDLEWARES
// =========================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();

app.UseRouting();
app.UseAuthentication();
app.UseValidateUserExists();
app.UseAuthorization();

// Redirigir 403 automáticamente
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 403)
    {
        context.Response.Redirect("/AccessDenied");
    }
});

app.MapRazorPages();

app.Run();
