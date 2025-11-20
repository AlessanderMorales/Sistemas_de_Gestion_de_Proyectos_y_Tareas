using System.Net.Http.Headers;

namespace Sistema_de_Gestion_de_Proyectos_y_Tareas.Handlers
{
    public class JwtAuthenticationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<JwtAuthenticationHandler> _logger;

        public JwtAuthenticationHandler(IHttpContextAccessor httpContextAccessor, ILogger<JwtAuthenticationHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                var token = httpContext.Session.GetString("JwtToken");

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogDebug($"Token JWT agregado a la petición: {request.RequestUri}");
                }
                else
                {
                    _logger.LogDebug($"No se encontró token JWT para la petición: {request.RequestUri}");
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
