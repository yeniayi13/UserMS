using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Database;
using UserMs.Core.Repositories.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.Repositories.ActivityHistory
{
    public class ActivityHistoryRepositoryMongo : IActivityHistoryRepositoryMongo
    {

        //private readonly IUserDbContext _dbContext;
        private readonly IMongoCollection<Domain.Entities.ActivityHistory.ActivityHistory> _collection;
        private readonly IMapper _mapper;


        public ActivityHistoryRepositoryMongo(IUserDbContextMongo context, IMapper mapper)
        {

            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            if (context?.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }

            _collection =
                context.Database.GetCollection<Domain.Entities.ActivityHistory.ActivityHistory>("ActivityHistories");
        }

        public async Task<List<Domain.Entities.ActivityHistory.ActivityHistory>> GetActivitiesByUserAsync(UserId userId,
            DateTime? startDate, DateTime? endDate)
        {
            Console.WriteLine($"Buscando historial de actividades para el usuario ID: {userId}");

            // Filtro de búsqueda
            var filter = Builders<Domain.Entities.ActivityHistory.ActivityHistory>.Filter.Eq("UserId", userId.Value);

            if (startDate.HasValue)
                filter = filter &
                         Builders<Domain.Entities.ActivityHistory.ActivityHistory>.Filter.Gte(a => a.Timestamp,
                             startDate.Value);
            if (endDate.HasValue)
                filter = filter &
                         Builders<Domain.Entities.ActivityHistory.ActivityHistory>.Filter.Lte(a => a.Timestamp,
                             endDate.Value);

            // Proyección para excluir Id si es necesario
            var projection = Builders<Domain.Entities.ActivityHistory.ActivityHistory>.Projection.Include(a => a.Action)
                .Include(a => a.Timestamp);

            // Ejecutar consulta y proyectar al DTO
            var activityDto = await _collection
                .Find(filter)
                .Project<GetActivityHistoryDto>(projection) // Convertir a DTO
                .ToListAsync()
                .ConfigureAwait(false);

            Console.WriteLine(activityDto.Count > 0
                ? $"Historial encontrado: {activityDto.Count} registros"
                : "No se encontraron actividades");

            // Mapear de DTO a entidad de negocio
            var activityEntity = _mapper.Map<List<Domain.Entities.ActivityHistory.ActivityHistory>>(activityDto);

            return activityEntity;
        }
    }
}
