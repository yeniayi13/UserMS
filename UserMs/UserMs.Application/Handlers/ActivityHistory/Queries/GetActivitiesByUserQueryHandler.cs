using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.HistoryActivity;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Domain.Entities;

namespace UserMs.Application.Handlers.ActivityHistory.Queries
{
    public class GetActivitiesByUserQueryHandler : IRequestHandler<GetActivitiesByUserQuery, List<GetActivityHistoryDto>>
    {
        private readonly IActivityHistoryRepositoryMongo _activityHistoryRepositoryMongo;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetActivitiesByUserQueryHandler> _logger;

        public GetActivitiesByUserQueryHandler(
            IActivityHistoryRepositoryMongo activityHistoryRepositoryMongo,
            IMapper mapper,
            ILogger<GetActivitiesByUserQueryHandler> logger, IActivityHistoryRepository activityHistoryRepository)
        {
            _activityHistoryRepository = activityHistoryRepository;
            _activityHistoryRepositoryMongo = activityHistoryRepositoryMongo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<GetActivityHistoryDto>> Handle(GetActivitiesByUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = UserId.Create(request.UserId);
                var activities = await _activityHistoryRepositoryMongo.GetActivitiesByUserAsync(userId, request.StartDate, request.EndDate);

                if (activities == null || !activities.Any())
                {
                    _logger.LogWarning($"No se encontró historial de actividades para el usuario ID: {request.UserId}");

                    // Registrar la acción de consulta en el historial aunque no haya datos
                    var logActivity = new Domain.Entities.ActivityHistory.ActivityHistory(
                        Guid.NewGuid(),
                        request.UserId,
                        "Consulta sin resultados en historial de actividades",
                        DateTime.UtcNow
                    );
                    await _activityHistoryRepository.AddAsync(logActivity);

                    return new List<GetActivityHistoryDto>();
                }

                // Registrar la acción de consulta en el historial
                var historyLog = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    request.UserId,
                    "Consulta de historial de actividades",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(historyLog);

                return _mapper.Map<List<GetActivityHistoryDto>>(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener historial de actividades para el usuario ID {request.UserId}: {ex.Message}");

                return new List<GetActivityHistoryDto>();
            }
        }
    }
}
