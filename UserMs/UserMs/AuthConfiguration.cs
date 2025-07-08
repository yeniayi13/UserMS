using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace UserMs
{
    [ExcludeFromCodeCoverage]
    public static class AuthConfiguration
    {
        public static IServiceCollection KeycloakConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configurar autenticación basada en JWT con Keycloak
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = configuration["Authentication:Audience"];
                    options.MetadataAddress = configuration.GetValue<string>("Authentication:MetadataAddress");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Authentication:ValidIssuer"],
                        ValidateAudience = true,
                        ValidAudiences = new[] { "account", "realm-management" },
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Agregar eventos para depuración
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "").Trim();
                            Console.WriteLine($"Token recibido: {token}");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Error de autenticación: {context.Exception}");
                            return Task.CompletedTask;
                        }
                    };
                });

            // Configurar políticas de autorización basadas en roles de Keycloak
            services.AddAuthorization(o =>
            {
                // ✅ Política: Administrador
                o.AddPolicy("AdministradorPolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            return resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess) &&
                                clientAccess.GetProperty("roles").EnumerateArray()
                                    .Any(role => role.GetString() == "Administrator");
                        }));

                // ✅ Política: Soporte
                o.AddPolicy("SoportePolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            return resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess) &&
                                clientAccess.GetProperty("roles").EnumerateArray()
                                    .Any(role => role.GetString() == "Support");
                        }));

                // ✅ Política: Postor
                o.AddPolicy("PostorPolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            return resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess) &&
                                clientAccess.GetProperty("roles").EnumerateArray()
                                    .Any(role => role.GetString() == "Bidder");
                        }));

                // ✅ Política: Subastador
                o.AddPolicy("SubastadorPolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            return resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess) &&
                                clientAccess.GetProperty("roles").EnumerateArray()
                                    .Any(role => role.GetString() == "Auctioneer");
                        }));

                // ✅ Política combinada: Subastador o Postor
                o.AddPolicy("SubastadorOPostorPolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            if (!resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess))
                                return false;

                            var roles = clientAccess.GetProperty("roles").EnumerateArray().Select(r => r.GetString());
                            return roles.Contains("Auctioneer") || roles.Contains("Bidder");
                        }));

                // ✅ Política combinada: Administrador o Soporte
                o.AddPolicy("AdministradorOSoportePolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            if (!resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess))
                                return false;

                            var roles = clientAccess.GetProperty("roles").EnumerateArray().Select(r => r.GetString());
                            return roles.Contains("Administrator") || roles.Contains("Support");
                        }));
            });

            // Agregar IHttpContextAccessor (necesario para acceder al contexto HTTP)
            services.AddHttpContextAccessor();

           

            return services;
        }
    }


}
