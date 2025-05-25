using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Database;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.ActivityHistory;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Infrastructure.Repositories.ActivityHistoryRepo
{
    public class ActivityHistoryRepository: IActivityHistoryRepository
    {

        private readonly IUserDbContext _dbContext;
        private readonly IMapper _mapper;


        public ActivityHistoryRepository(IUserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

           
        }
        public async Task AddAsync(Domain.Entities.ActivityHistory.ActivityHistory activity)
        {
            await _dbContext.ActivityHistories.AddAsync(activity);
            await _dbContext.SaveEfContextChanges("");
        }

        


    }


}
