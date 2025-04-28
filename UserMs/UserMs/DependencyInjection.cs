using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserMs.Core.Interface;
using UserMs.Infrastructure;

namespace UserMs
{
    internal static class DependencyInjection
    {
        internal static IServiceCollection KeycloakConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configura la autenticación basada en JWT para validar los tokens emitidos por Keycloak 
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = configuration["Authentication:Audience"];
                    options.MetadataAddress = configuration["Authentication:MetadataAddress"]!;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["Authentication:ValidateIssuer"],
                    };
                });

            services.AddAuthorization(o =>
            {


                o.AddPolicy("AdminWebOnly", policy =>
         policy.RequireAuthenticatedUser()
               .RequireAssertion(context =>
               {
                   var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                   if (string.IsNullOrEmpty(resourceAccess))
                       return false;

                   // Parsear el JSON de resource_access
                   var resourceAccessJson = System.Text.Json.JsonDocument.Parse(resourceAccess);
                   if (resourceAccessJson.RootElement.TryGetProperty("webclient", out var webClientAccess))
                   {
                       var rol = webClientAccess.GetProperty("roles").EnumerateArray()
                                             .Any(role => role.GetString() == "Administrator" || role.GetString() == "Auctioneer" || role.GetString() == "Bidder" || role.GetString() == "Support");
                       Console.WriteLine("Rol:" + rol);
                       return rol;
                   }

                   return false;
               }));

            });

            services.AddAuthorization(o =>
            {


                o.AddPolicy("AuctioneerBidderOnly", policy =>
         policy.RequireAuthenticatedUser()
               .RequireAssertion(context =>
               {
                   var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                   if (string.IsNullOrEmpty(resourceAccess))
                       return false;

                   // Parsear el JSON de resource_access
                   var resourceAccessJson = System.Text.Json.JsonDocument.Parse(resourceAccess);
                   if (resourceAccessJson.RootElement.TryGetProperty("webclient", out var webClientAccess))
                   {
                       var rol = webClientAccess.GetProperty("roles").EnumerateArray()
                                             .Any(role =>  role.GetString() == "Auctioneer" || role.GetString() == "Bidder" );
                       Console.WriteLine("Rol:" + rol);
                       return rol;
                   }

                   return false;
               }));

            });



            services.AddAuthorization(o =>
            {


                o.AddPolicy("AdminSupportOnly", policy =>
         policy.RequireAuthenticatedUser()
               .RequireAssertion(context =>
               {
                   var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                   if (string.IsNullOrEmpty(resourceAccess))
                       return false;

                   // Parsear el JSON de resource_access
                   var resourceAccessJson = System.Text.Json.JsonDocument.Parse(resourceAccess);
                   if (resourceAccessJson.RootElement.TryGetProperty("webclient", out var webClientAccess))
                   {
                       var rol = webClientAccess.GetProperty("roles").EnumerateArray()
                                             .Any(role => role.GetString() == "Administrator" || role.GetString() == "Support");
                       Console.WriteLine("Rol:" + rol);
                       return rol;
                   }

                   return false;
               }));

            });

            services.AddAuthorization(o =>
            {


                o.AddPolicy("BidderOnly", policy =>
         policy.RequireAuthenticatedUser()
               .RequireAssertion(context =>
               {
                   var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                   if (string.IsNullOrEmpty(resourceAccess))
                       return false;

                   // Parsear el JSON de resource_access
                   var resourceAccessJson = System.Text.Json.JsonDocument.Parse(resourceAccess);
                   if (resourceAccessJson.RootElement.TryGetProperty("webclient", out var webClientAccess))
                   {
                       var rol = webClientAccess.GetProperty("roles").EnumerateArray()
                                             .Any(role => role.GetString() == "Bidder");
                       Console.WriteLine("Rol:" + rol);
                       return rol;
                   }

                   return false;
               }));
            });


            // Agrega IHttpContextAccessor (necesario para acceder al contexto HTTP)
            services.AddHttpContextAccessor();
            // Registra el DelegatingHandler

            services.AddHttpClient<IAuthMsService, AuthMsService>();


            return services;
        }
    }
}