using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Test.Infrastructure.Service
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _response;
        private readonly bool _simulateTimeout;

        public MockHttpMessageHandler(HttpResponseMessage response, bool simulateTimeout = false)
        {
            _response = response;
            _simulateTimeout = simulateTimeout;
        }

        public void SetResponse(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_simulateTimeout)
            {
                await Task.Delay(5000, cancellationToken); // 🔹 Simula retraso en la respuesta

                // 🔹 Simula la cancelación de la tarea
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException("La solicitud fue cancelada debido a un timeout.");
                }
            }

            return await Task.FromResult(_response); // 🔹 Retorna la respuesta simulada
        }
    }
}
