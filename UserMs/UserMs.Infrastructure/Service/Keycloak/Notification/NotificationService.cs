using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using UserMs.Common.Dtos.Response;
using ClaimsMS.Core.Service.Notification;

namespace UserMs.Infrastructure.Service.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _httpClientUrl;

        public NotificationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor,
            IOptions<HttpClientUrl> httpClientUrl)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _httpClientUrl = httpClientUrl.Value.ApiUrl;

            //* Configuracion del HttpClient
            var headerToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()
                ?.Replace("Bearer ", "");
            _httpClient.BaseAddress = new Uri("http://localhost:18086/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {headerToken}");
        }

        public async Task SendNotificationAsync(GetNotificationDto notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "La notificación no puede ser nula.");
            }

            var content = new StringContent(
                JsonSerializer.Serialize(notification),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"notification/Add-Notification", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al enviar la notificación: {response.StatusCode}");
            }

            Console.WriteLine($"Notificación enviada con éxito: {notification.NotificationId}");
        }
    }
}
