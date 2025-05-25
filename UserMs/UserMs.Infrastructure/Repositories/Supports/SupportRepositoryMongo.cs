using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.Database;
using UserMs.Core.Repositories.Supports;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.Repositories.Supports
{
    public class SupportRepositoryMongo : ISupportRepositoryMongo
    {
       
        private readonly IMongoCollection<Domain.Entities.Support.Supports> _collection;
        private readonly IMapper _mapper;

        public SupportRepositoryMongo(IUserDbContextMongo context, IMapper mapper)
        {
            
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            if (context?.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }

            _collection = context.Database.GetCollection<Domain.Entities.Support.Supports>("Supports");
        }

        public async Task<List<Domain.Entities.Support.Supports>> GetSupportAllAsync()
        {
            var projection = Builders<Domain.Entities.Support.Supports>.Projection.Exclude("_id").Exclude("IsDeleted").Exclude("CreatedAt").Exclude("CreatedBy");

            var supportDto = await _collection.Find(Builders<Domain.Entities.Support.Supports>.Filter.Empty)
                .Project<GetSupportDto>(projection)
                .ToListAsync()
                .ConfigureAwait(false);

            return supportDto == null || supportDto.Count == 0 ? new List<Domain.Entities.Support.Supports>() : _mapper.Map<List<Domain.Entities.Support.Supports>>(supportDto);
        }

        public async Task<Domain.Entities.Support.Supports?> GetSupportByIdAsync(UserId supportId)
        {
            var filter = Builders<Domain.Entities.Support.Supports>.Filter.Eq("UserId", supportId.Value);
            var projection = Builders<Domain.Entities.Support.Supports>.Projection.Exclude("_id").Exclude("IsDeleted").Exclude("CreatedAt").Exclude("CreatedBy");

            var supportDto = await _collection.Find(filter).Project<GetSupportDto>(projection).FirstOrDefaultAsync().ConfigureAwait(false);
            return _mapper.Map<Domain.Entities.Support.Supports>(supportDto);
        }

        public async Task<Domain.Entities.Support.Supports?> GetSupportByEmailAsync(UserEmail supportUserEmail)
        {
            var filter = Builders<Domain.Entities.Support.Supports>.Filter.Eq("UserEmail", supportUserEmail.Value);
            var projection = Builders<Domain.Entities.Support.Supports>.Projection.Exclude("_id").Exclude("IsDeleted").Exclude("CreatedAt").Exclude("CreatedBy");

            var supportDto = await _collection.Find(filter).Project<GetSupportDto>(projection).FirstOrDefaultAsync().ConfigureAwait(false);
            return _mapper.Map<Domain.Entities.Support.Supports>(supportDto);
        }

      
    }
}
