using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.ApiClients;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Application.Facades;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Middleware;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Filters;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Handlers;
using Sistema_de_Gestion_de_Proyectos_y_Tareas.Service;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("es-ES")
{
    DateTimeFormat =
    {
        ShortDatePattern = "dd/MM/yyyy",
        DateSeparator = "/"
    }
};

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(cultureInfo);
    options.SupportedCultures = new[] { cultureInfo };
    options.SupportedUICultures = new[] { cultureInfo };
});

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
    options.AddPolicy("SoloAdmin", policy => policy.RequireRole("SuperAdmin"));

    options.AddPolicy("OnlyJefeOrEmpleado", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("JefeDeProyecto") ||
            ctx.User.IsInRole("Empleado") ||
            ctx.User.IsInRole("SuperAdmin")));

    options.AddPolicy("OnlyJefe", policy => policy.RequireRole("JefeDeProyecto"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EmailService>();
builder.Services.AddTransient<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<UsuarioApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:UsuarioApi"]);
}).AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<ProyectoApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:ProyectoApi"]);
}).AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<TareaApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:TareaApi"]);
}).AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddHttpClient<ComentarioApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:ComentarioApi"]);
}).AddHttpMessageHandler<JwtAuthenticationHandler>();

builder.Services.AddScoped<GestionProyectosFacade>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Usuarios", "SoloAdmin");
    options.Conventions.AuthorizeFolder("/Proyectos", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizeFolder("/Tareas", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizeFolder("/Comentarios", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizePage("/Index", "OnlyJefeOrEmpleado");
    options.Conventions.AuthorizeFolder("/Configuracion");
    options.Conventions.AllowAnonymousToPage("/Login/Login");
    options.Conventions.AllowAnonymousToPage("/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Logout");
})
.AddMvcOptions(options =>
{
    options.Filters.Add<RequirePasswordChangeFilter>();
});

var app = builder.Build();

// Configurar HTTPS Redirection con puerto específico
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseValidateUserExists();
app.UseAuthorization();

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
